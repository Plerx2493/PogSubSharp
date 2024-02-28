using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PogSubSharp.Models;
using PogSubSharp.Models.Events;
using PogSubSharp.Models.EventSub;
using PogSubSharp.Models.EventSub.Events;

namespace PogSubSharp;

public class EventSubWebSocket : IAsyncDisposable
{
    private readonly ClientWebSocket _client;
    private readonly byte[] _receiveRawBuffer = new byte[4096];
    private readonly MemoryStream _receiveStream = new();
    private readonly SemaphoreSlim _receiveLock = new(1, 1);
    private readonly CancellationToken _ct;
    private readonly ILogger _logger;
    private int _keepAliveInterval;
    private readonly bool IsDisposed = false;
    private readonly CancellationTokenSource _backgroundCts = new();
    private DateTimeOffset _lastKeepAlive;

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
        var builder = new UriBuilder(uri ?? new Uri("wss://eventsub.wss.twitch.tv/ws"));
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
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", _ct);
        await _backgroundCts.CancelAsync();
    }

    private async Task<WebSocketFrame> ReceiveAsync()
    {
        if (_client.State != WebSocketState.Open && _client.State == WebSocketState.CloseSent)
        {
            return WebSocketFrame.Closed;
        }
        
        await _receiveLock.WaitAsync(_ct);
        
        _receiveStream.SetLength(0);

        try
        {
            WebSocketReceiveResult? result;
            do
            {
                if (_client.State != WebSocketState.Open && _client.State == WebSocketState.CloseSent)
                {
                    return WebSocketFrame.Closed;
                }
                result = await _client.ReceiveAsync(_receiveRawBuffer, CancellationToken.None);
                _receiveStream.Write(_receiveRawBuffer, 0, result.Count);
            } while (!result.EndOfMessage);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        
        if (_receiveStream.Length == 0)
        {
            _receiveLock.Release();
            return WebSocketFrame.Empty;
        }
        
        _receiveStream.Seek(0, SeekOrigin.Begin);
        _logger.LogDebug("Received {Bytes} bytes", _receiveStream.Length);
        _logger.LogTrace("Received {Data}", Encoding.UTF8.GetString(_receiveStream.ToArray()));
        
        _receiveStream.Seek(0, SeekOrigin.Begin);
        var frame = JsonSerializer.Deserialize<EventSubFrame>(_receiveStream)!;
        
        _receiveLock.Release();

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
            WebSocketFrame rawFrame;
            
            try
            {
                rawFrame = await ReceiveAsync();
            }
            catch (Exception e)
            {
                OnErrorAsync(e);
                continue;
            }
            
            if (rawFrame.IsClosed)
            {
                _logger.LogWarning("WebSocket was closed");
                break;
            }
            
            if (rawFrame.IsEmpty)
            {
                continue;
            }
            
            EventSubFrame frame = rawFrame.Frame!;
            
            _lastKeepAlive = frame.Metadata.Timestamp;

            switch (frame.Metadata.Type)
            {
                case EventSubMessageType.session_welcome:
                    ArgumentNullException.ThrowIfNull(frame.Payload?.Session);
                    _ = Task.Run(() => OnSessionWelcomeAsync(frame.Payload?.Session!), _ct);
                    break;
                case EventSubMessageType.session_reconnect:
                    _ = Task.Run(() => OnSessionReconnectAsync(frame.Payload?.Session!), _ct);                    
                    break;
                case EventSubMessageType.session_keepalive:
                    _ = Task.Run(OnSessionKeepAliveAsync, _ct);
                    break;
                case EventSubMessageType.notification:
                    _ = Task.Run(() => OnNotificationAsync(frame.Payload?.Subscription!, frame.Payload?.Event!), _ct);
                    break;
                case EventSubMessageType.revocation:
                    _ = Task.Run(() => OnRevocationAsync(frame.Payload?.Subscription!), _ct);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void  OnRevocationAsync(EventSubSubscription payloadSubscription)
    {
        OnRevocation?.Invoke(this, new RevocationEventArgs
        {
            Subscription = payloadSubscription
        });
    }

    private void OnNotificationAsync(EventSubSubscription payloadSubscription, IEventSubEvent payloadEvent)
    {
        OnNotification?.Invoke(this, new NotificationEventArgs
        {
            Subscription = payloadSubscription,
            Event = payloadEvent
        });
    }

    private void OnSessionKeepAliveAsync()
    {
        OnSessionKeepAlive?.Invoke(this, new SessionKeepAliveEventArgs());
    }

    private void OnSessionReconnectAsync(EventSocketsSession payloadSession)
    {
        OnSessionReconnect?.Invoke(this, new SessionReconnectEventArgs
        {
            Session = payloadSession
        });
    }

    private void OnSessionWelcomeAsync(object session)
    {
        OnSessionWelcome?.Invoke(this, new SessionWelcomeEventArgs
        {
            Session = (EventSocketsSession) session
        });
    }

    private void OnErrorAsync(Exception exception)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", _ct);
        }
        catch (Exception)
        {
            if (IsDisposed)
            {
                return;
            }
        }
        
        _client.Dispose();
        await _receiveStream.DisposeAsync();
        _receiveLock.Dispose();
    }
}