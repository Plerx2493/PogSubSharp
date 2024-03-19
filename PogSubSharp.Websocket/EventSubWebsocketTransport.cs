using Microsoft.Extensions.Logging;
using PogSubSharp.Shared.Events;
using PogSubSharp.Shared.Notifications.EventSubNotifications;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

public class EventSubWebsocketTransport : IEventSubTransport
{
    private EventSubWebSocket _webSocket;
    private ILogger _logger;
    private string _uri;
    private string? SessionId;

    public EventSubWebsocketTransport(ILogger logger, string uri = "wss://eventsub.wss.twitch.tv/ws" )
    {
        _logger = logger;
        _uri = uri;
        _webSocket = new EventSubWebSocket(logger);
    }

    public ValueTask SetupAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask StartAsync()
    {
        await ConnectAsync();
    }

    public async ValueTask StopAsync()
    {
        await DisconnectAsync();
    }

    public ValueTask TearDownAsync()
    {
        return ValueTask.CompletedTask;
    }

    public EventSubTransport GetCurrentTransport()
    {
        return new EventSubTransport()
        {
            Method = EventSubMethod.websocket,
            SessionId = this.SessionId
        };
    }

    public event EventHandler<NotificationEventArgs>? OnNotification;
    public event EventHandler<RevocationEventArgs>? OnRevocation;
    public event EventHandler<Exception>? OnError;

    private async Task ConnectAsync()
    {
        _webSocket.OnError += OnErrorAsync;
        _webSocket.OnRevocation += OnRevocationAsync;
        _webSocket.OnNotification += OnNotificationAsync;
        _webSocket.OnSessionKeepAlive += OnSessionKeepAliveAsync;
        _webSocket.OnSessionReconnect += OnSessionReconnectAsync;
        _webSocket.OnSessionWelcome += OnSessionWelcomeAsync;
        
        Uri wsUri = new(_uri);
        await _webSocket.ConnectAsync(wsUri);

        // wait for welcome message on new socket
        await WaitForWelcomeAsync();
    }

    private async Task DisconnectAsync()
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
        await WaitForWelcomeAsync();
        
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
    }
    
    private void OnErrorAsync(object? sender, Exception exception)
    {
        _logger.LogError(exception, "An error occurred while receiving from the WebSocket");
    }

    private void OnRevocationAsync(object? sender, RevocationEventArgs revocationEventArgs)
    {
        EventSubSubscription payloadSubscription = revocationEventArgs.Subscription;
        _logger.LogWarning("Subscription {SubscriptionId} was revoked", payloadSubscription.Id);
    }

    private void OnNotificationAsync(object? sender, NotificationEventArgs notificationEventArgs)
    {
       OnNotification?.Invoke(this, notificationEventArgs);
    }

    private void OnSessionKeepAliveAsync(object? sender, SessionKeepAliveEventArgs sessionKeepAliveEventArgs)
    {
        _logger.LogDebug("PogSubSharp.Websocket: Received keepalive");
    }

    private async void OnSessionReconnectAsync(object? sender, SessionReconnectEventArgs sessionReconnectEventArgs)
    {
        EventSocketsSession session = sessionReconnectEventArgs.Session;
        _logger.LogInformation("Reconnecting to session {SessionId}", session.Id);
        
        Uri reconnectUri = new(session.ReconnectUrl!);
        await HandleReconnectAsync(reconnectUri);
    }

    private void OnSessionWelcomeAsync(object? sender, SessionWelcomeEventArgs sessionWelcomeEventArgs)
    {
        EventSocketsSession session = sessionWelcomeEventArgs.Session;
        SessionId = session.Id;
        _logger.LogInformation("Connected to session {SessionId}", session.Id);
    }
    
    private async Task WaitForWelcomeAsync()
    {
        TaskCompletionSource? ct = new();
        _webSocket.OnSessionWelcome += WelcomeReceived;
        await ct.Task;
        _webSocket.OnSessionWelcome -= WelcomeReceived;
        
        void WelcomeReceived(object? _, SessionWelcomeEventArgs __)
        {
            ct.SetResult();
        }
    }
}