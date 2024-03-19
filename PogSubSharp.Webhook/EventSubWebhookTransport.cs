using PogSubSharp.Shared.Events;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Webhook;

public class EventSubWebhookTransport : IEventSubTransport
{
    public ValueTask SetupAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask StartAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask StopAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask TearDownAsync()
    {
        throw new NotImplementedException();
    }

    public EventSubTransport GetCurrentTransport()
    {
        throw new NotImplementedException();
    }

    public event EventHandler<NotificationEventArgs>? OnNotification;
    public event EventHandler<RevocationEventArgs>? OnRevocation;
    public event EventHandler<Exception>? OnError;
}