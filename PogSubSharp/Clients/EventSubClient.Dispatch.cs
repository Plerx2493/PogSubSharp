using Microsoft.Extensions.Logging;
using PogSubSharp.EventSub;
using PogSubSharp.EventSub.Events;
using PogSubSharp.EventSub.Transport;
using PogSubSharp.Notifications;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    public async Task ConnectAsync(string uri)
    {
        if (!NotificationHandler.IsReady)
        {
            throw new InvalidOperationException("NotificationHandler is not ready");
        }
        
        Uri wsUri = new(uri);
        await _webSocket.ConnectAsync(wsUri);
        
        _webSocket.OnError += OnErrorAsync;
        _webSocket.OnRevocation += OnRevocationAsync;
        _webSocket.OnNotification += OnNotificationAsync;
        _webSocket.OnSessionKeepAlive += OnSessionKeepAliveAsync;
        _webSocket.OnSessionReconnect += OnSessionReconnectAsync;
        _webSocket.OnSessionWelcome += OnSessionWelcomeAsync;
    }
    
    public async Task DisconnectAsync()
    {
        await _webSocket.DisconnectAsync();
        
        _webSocket.OnError -= OnErrorAsync;
        _webSocket.OnRevocation -= OnRevocationAsync;
        _webSocket.OnNotification -= OnNotificationAsync;
        _webSocket.OnSessionKeepAlive -= OnSessionKeepAliveAsync;
        _webSocket.OnSessionReconnect -= OnSessionReconnectAsync;
        _webSocket.OnSessionWelcome -= OnSessionWelcomeAsync;
    }

    private async Task HandleReconnectAsync(Uri uri)
    {
        EventSubWebSocket oldWebSocket = _webSocket;
        _webSocket = new EventSubWebSocket(_logger);
        
        // setup new event handlers
        _webSocket.OnError += OnErrorAsync;
        _webSocket.OnRevocation += OnRevocationAsync;
        _webSocket.OnNotification += OnNotificationAsync;
        _webSocket.OnSessionKeepAlive += OnSessionKeepAliveAsync;
        _webSocket.OnSessionReconnect += OnSessionReconnectAsync;
        _webSocket.OnSessionWelcome += OnSessionWelcomeAsync;
        
        // connect to new uri
        await _webSocket.ConnectAsync(uri);
        
        // wait for welcome message on new socket
        TaskCompletionSource? ct = new TaskCompletionSource();
        _webSocket.OnSessionWelcome += WelcomeReceived;
        await ct.Task;
        _webSocket.OnSessionWelcome -= WelcomeReceived;
        
        // disconnect old socket
        await oldWebSocket.DisconnectAsync();
        
        // remove old event handlers
        oldWebSocket.OnError -= OnErrorAsync;
        oldWebSocket.OnRevocation -= OnRevocationAsync;
        oldWebSocket.OnNotification -= OnNotificationAsync;
        oldWebSocket.OnSessionKeepAlive -= OnSessionKeepAliveAsync;
        oldWebSocket.OnSessionReconnect -= OnSessionReconnectAsync;
        oldWebSocket.OnSessionWelcome -= OnSessionWelcomeAsync;
        
        await oldWebSocket.DisposeAsync();

        void WelcomeReceived(object? _, SessionWelcomeEventArgs __)
        {
            ct.SetResult();
        }
    }

    private async Task HandleKeepAliveTimeout()
    {
        
    }

    private async void OnErrorAsync(object? sender, Exception exception)
    {
        _logger.LogError(exception, "An error occurred while receiving from the WebSocket");
    }

    private async void OnRevocationAsync(object? sender, RevocationEventArgs revocationEventArgs)
    {
        EventSubSubscription payloadSubscription = revocationEventArgs.Subscription;
        _logger.LogWarning("Subscription {SubscriptionId} was revoked", payloadSubscription.Id);
    }

    private async void OnNotificationAsync(object? sender, NotificationEventArgs notificationEventArgs)
    {
        EventSubSubscription payloadSubscription = notificationEventArgs.Subscription;
        IEventSubNotification payloadEvent = notificationEventArgs.Event;
       _logger.LogInformation("Received notification for subscription {SubscriptionId}", payloadSubscription.Id);
       
       _ = NotificationHandler.HandleNotificationAsync(payloadEvent);
    }

    private async void OnSessionKeepAliveAsync(object? sender, SessionKeepAliveEventArgs sessionKeepAliveEventArgs)
    {
        _logger.LogInformation("Received keepalive");
    }

    private async void OnSessionReconnectAsync(object? sender, SessionReconnectEventArgs sessionReconnectEventArgs)
    {
        EventSocketsSession session = sessionReconnectEventArgs.Session;
        _logger.LogInformation("Reconnecting to session {SessionId}", session.Id);
        
        Uri reconnectUri = new Uri(session.ReconnectUrl!);
        await HandleReconnectAsync(reconnectUri);
    }

    private async void OnSessionWelcomeAsync(object? sender, SessionWelcomeEventArgs sessionWelcomeEventArgs)
    {
        EventSocketsSession session = sessionWelcomeEventArgs.Session;
        _logger.LogInformation("Connected to session {SessionId}", session.Id);
    }
}