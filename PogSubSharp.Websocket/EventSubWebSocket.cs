using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Microsoft.Extensions.Logging;
using PogSubSharp.Shared.Events;
using PogSubSharp.Shared.Notifications.EventSubNotifications;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

public class EventSubWebSocket : IAsyncDisposable
{
    private readonly ClientWebSocket _client;
    private readonly byte[] _receiveRawBuffer = new byte[4096];
    private readonly ArrayPoolBufferWriter<byte> _receiveBuffer = new();
    private readonly CancellationToken _ct;
    private readonly ILogger _logger;
    private readonly int _keepAliveInterval;
    private readonly CancellationTokenSource _backgroundCts = new();
    private readonly Queue<string> ReceivedMessages = new(1000);
    private DateTimeOffset _lastKeepAlive;
    private bool IsDisposed;
    
    private bool isConnected => _client.State == WebSocketState.Open;

    public EventSubWebSocket(ILogger logger, int keepAliveInterval = 10, CancellationToken ct = default)
    {
        _logger = logger;
        _ct = ct;
        _keepAliveInterval = keepAliveInterval;
        _client = new ClientWebSocket();
    }

    #region events

    public event EventHandler<SessionWelcomeEventArgs> OnSessionWelcome;
    public event EventHandler<SessionReconnectEventArgs> OnSessionReconnect;
    public event EventHandler<SessionKeepAliveEventArgs> OnSessionKeepAlive;
    public event EventHandler<NotificationEventArgs> OnNotification;
    public event EventHandler<RevocationEventArgs> OnRevocation;
    public event EventHandler<Exception> OnError; 

    #endregion
    
    public async Task ConnectAsync(Uri? uri = null)
    {
        if (isConnected)
        {
            throw new InvalidOperationException("The WebSocket is already connected");
        }
        
        UriBuilder builder = new(uri ?? new Uri("wss://eventsub.wss.twitch.tv/ws"));
        builder.Query = $"keepalive_timeout_seconds={_keepAliveInterval}";
        await _client.ConnectAsync(builder.Uri, _ct);
        
        _ = Task.Factory.StartNew(
            HandleReceive,
            _backgroundCts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
        
        _ = Task.Factory.StartNew(
            HandleKeepAlive,
            _backgroundCts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
    }
    
    public async Task DisconnectAsync()
    {
        if (!isConnected)
        {
            return;
        }
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", _ct);
        await _backgroundCts.CancelAsync();
    }

    private async Task<WebSocketFrame> ReceiveAsync()
    {
        if (_client.State != WebSocketState.Open && _client.State == WebSocketState.CloseSent)
        {
            return WebSocketFrame.Closed;
        }
        
        try
        {
            WebSocketReceiveResult? result;
            do
            {
                if (_client.State != WebSocketState.Open && _client.State == WebSocketState.CloseSent)
                {
                    return WebSocketFrame.Closed;
                }
                result = await _client.ReceiveAsync(_receiveRawBuffer, _backgroundCts.Token);
                _receiveBuffer.Write((ReadOnlySpan<byte>)_receiveRawBuffer.AsSpan().Slice(0, result.Count));
            } while (!result.EndOfMessage);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        
        if (_receiveBuffer.WrittenCount == 0)
        {
            return WebSocketFrame.Empty;
        }
        
#if DEBUG
        
        _logger.LogDebug("Received {Bytes} bytes", _receiveBuffer.WrittenCount);
        _logger.LogTrace("Received {Data}", Encoding.UTF8.GetString(_receiveBuffer.WrittenSpan));
        
#endif
        EventSubFrame frame;
        
        try
        {
            frame = JsonSerializer.Deserialize<EventSubFrame>(_receiveBuffer.WrittenSpan)!;
        }
        catch (Exception e)
        {
            _receiveBuffer.Clear();
            return WebSocketFrame.FromException(e);
        }
        
        _receiveBuffer.Clear();

        return new WebSocketFrame
        {
            Frame = frame,
            IsClosed = false,
            IsEmpty = false
        };
    }
    
    private async Task HandleKeepAlive()
    {
        while (!IsDisposed)
        {
            await Task.Delay(_keepAliveInterval * 1000);
            if (DateTimeOffset.UtcNow - _lastKeepAlive > TimeSpan.FromSeconds(_keepAliveInterval + 5))
            {
                _logger.LogWarning("WebSocket keepalive timeout");
                await DisconnectAsync();
                
                OnError?.Invoke(this, new Exception("WebSocket keepalive timeout"));
                break;
            }
        }
    }
    
    private async Task HandleReceive()
    {
        while (!IsDisposed)
        {
            WebSocketFrame rawFrame = await ReceiveAsync();
            
            if (rawFrame.IsClosed)
            {
                _logger.LogWarning("WebSocket was closed");
                break;
            }
            
            if (rawFrame.IsEmpty)
            {
                continue;
            }

            if (rawFrame.IsException)
            {
                _logger.LogError(rawFrame.Exception, "An error occurred while receiving from the WebSocket");
                continue;
            }
            
            EventSubFrame frame = rawFrame.Frame!;
            
            _lastKeepAlive = frame.Metadata.Timestamp;
            
            if (ReceivedMessages.Contains(frame.Metadata.Id))
            {
                _logger.LogInformation("Received duplicate message with id {Id}", frame.Metadata.Id);
                continue;
            }
            
            ReceivedMessages.Enqueue(frame.Metadata.Id);
            if (ReceivedMessages.Count > 1000)
            {
                ReceivedMessages.Dequeue();
            }

            switch (frame.Metadata.Type)
            {
                case EventSubMessageType.session_welcome:
                    ArgumentNullException.ThrowIfNull(frame.Payload?.Session);
                    _ = Task.Run(() => HandleSessionWelcome(frame.Payload?.Session!), _ct);
                    break;
                case EventSubMessageType.session_reconnect:
                    _ = Task.Run(() => HandleSessionReconnect(frame.Payload?.Session!), _ct);                    
                    break;
                case EventSubMessageType.session_keepalive:
                    _ = Task.Run(HandleSessionKeepAlive, _ct);
                    break;
                case EventSubMessageType.notification:
                    _ = Task.Run(() => HandleNotification(frame.Payload?.Subscription!, frame.Payload?.Event!), _ct);
                    break;
                case EventSubMessageType.revocation:
                    _ = Task.Run(() => HandleRevocation(frame.Payload?.Subscription!), _ct);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void  HandleRevocation(EventSubSubscription payloadSubscription)
    {
        OnRevocation?.Invoke(this, new RevocationEventArgs
        {
            Subscription = payloadSubscription
        });
    }

    private void HandleNotification(EventSubSubscription payloadSubscription, IEventSubNotification? payloadEvent)
    {
        if (payloadEvent is null)
        {
            _logger.LogWarning("Received notification but could not deserialize it. Type: {Type}", payloadSubscription?.Type);
            return;
        }
        
        OnNotification?.Invoke(this, new NotificationEventArgs
        {
            Subscription = payloadSubscription,
            Event = payloadEvent
        });
    }

    private void HandleSessionKeepAlive()
    {
        OnSessionKeepAlive?.Invoke(this, new SessionKeepAliveEventArgs());
    }

    private void HandleSessionReconnect(EventSocketsSession payloadSession)
    {
        OnSessionReconnect?.Invoke(this, new SessionReconnectEventArgs
        {
            Session = payloadSession
        });
    }

    private void HandleSessionWelcome(EventSocketsSession session)
    {
        OnSessionWelcome?.Invoke(this, new SessionWelcomeEventArgs
        {
            Session = session
        });
    }

    private void OnErrorAsync(Exception exception)
    {
        OnError?.Invoke(this, exception);
    }

    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
        {
            return;
        }
        
        try
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", _ct);
        }
        catch (Exception)
        {
            // ignore
        }
        
        _client.Dispose();
        _receiveBuffer.Dispose();
        _backgroundCts.Dispose();
        
        IsDisposed = true;
    }
}