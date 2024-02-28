using Microsoft.Extensions.Logging;
using PogSubSharp.Models.EventSub.Events;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    public async Task ConnectAsync(string uri)
    {
        var wsUri = new Uri(uri);
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
        var oldWebSocket = _webSocket;
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
        var ct = new TaskCompletionSource();
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
        var payloadSubscription = revocationEventArgs.Subscription;
        _logger.LogWarning("Subscription {SubscriptionId} was revoked", payloadSubscription.Id);
    }

    private async void OnNotificationAsync(object? sender, NotificationEventArgs notificationEventArgs)
    {
        var payloadSubscription = notificationEventArgs.Subscription;
        var payloadEvent = notificationEventArgs.Event;
       _logger.LogInformation("Received notification for subscription {SubscriptionId}", payloadSubscription.Id);
    }

    private async void OnSessionKeepAliveAsync(object? sender, SessionKeepAliveEventArgs sessionKeepAliveEventArgs)
    {
        _logger.LogInformation("Received keepalive");
    }

    private async void OnSessionReconnectAsync(object? sender, SessionReconnectEventArgs sessionReconnectEventArgs)
    {
        var session = sessionReconnectEventArgs.Session;
        _logger.LogInformation("Reconnecting to session {SessionId}", session.Id);
        
        var reconnectUri = new Uri(session.ReconnectUrl!);
        await HandleReconnectAsync(reconnectUri);
    }

    private async void OnSessionWelcomeAsync(object? sender, SessionWelcomeEventArgs sessionWelcomeEventArgs)
    {
        var session = sessionWelcomeEventArgs.Session;
        _logger.LogInformation("Connected to session {SessionId}", session.Id);
    }
}