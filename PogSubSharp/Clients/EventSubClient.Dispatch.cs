using PogSubSharp.Models.Events;
using PogSubSharp.Models.EventSub;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    public EventSocketsSession Session { get; private set; }
    private DateTimeOffset _lastKeepAlive;

    private Task? receiverTask;
    private Task? keepAliveTask;
    
    public async Task ConnectAsync()
    {
        await _webSocket.ConnectAsync();
        receiverTask = HandleReceive();
        keepAliveTask = HandleKeepAlive();
    }
    
    public async Task DisconnectAsync()
    {
        await _webSocket.DisconnectAsync();
        isCanceled = true;
        
        receiverTask?.Dispose();
        keepAliveTask?.Dispose();
        
        receiverTask = null;
        keepAliveTask = null;
    }
    
    private async Task HandleReceive()
    {
        while (!isCanceled)
        {
            EventSubFrame frame;
            
            try
            {
                frame = await _webSocket.ReceiveAsync();
            }
            catch (Exception e)
            {
                _ = OnErrorAsync(e);
                continue;
            }
            
            _lastKeepAlive = frame.Metadata.Timestamp;

            switch (frame.Metadata.Type)
            {
                case EventSubMessageType.session_welcome:
                    ArgumentNullException.ThrowIfNull(frame.Payload.Session);
                    Session = frame.Payload.Session!;
                    _ = OnSessionWelcomeAsync(Session);
                    break;
                case EventSubMessageType.session_reconnect:
                    _ = OnSessionReconnectAsync(frame.Payload.Session!);                    
                    break;
                case EventSubMessageType.session_keepalive:
                    _ = OnSessionKeepAliveAsync();
                    break;
                case EventSubMessageType.notification:
                    _ = OnNotificationAsync(frame.Payload.Subscription!, frame.Payload.Event!);
                    break;
                case EventSubMessageType.revocation:
                    _ = OnRevocationAsync(frame.Payload.Subscription!);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
        }

            
    }

    private async Task OnErrorAsync(Exception exception)
    {
        throw new NotImplementedException();
    }

    private async Task OnRevocationAsync(EventSubSubscription payloadSubscription)
    {
        throw new NotImplementedException();
    }

    private async Task OnNotificationAsync(EventSubSubscription payloadSubscription, IEventSubEvent payloadEvent)
    {
        throw new NotImplementedException();
    }

    private async Task OnSessionKeepAliveAsync()
    {
        throw new NotImplementedException();
    }

    private async Task OnSessionReconnectAsync(EventSocketsSession session)
    {
        throw new NotImplementedException();
    }

    private async Task OnSessionWelcomeAsync(EventSocketsSession session)
    {
        
    }
    
    private async Task HandleKeepAlive()
    {
        while (!isCanceled)
        {
            await Task.Delay(Session.KeepaliveTimeoutSeconds * 1000);
            if (DateTimeOffset.UtcNow - _lastKeepAlive > TimeSpan.FromSeconds(Session.KeepaliveTimeoutSeconds + 5))
            {
                await _webSocket.DisconnectAsync();
                await _webSocket.ConnectAsync();
            }
        }
    }
}