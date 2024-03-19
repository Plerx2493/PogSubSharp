using PogSubSharp.Shared.Events;

namespace PogSubSharp.Shared.Transport;

public interface IEventSubTransport
{
    public ValueTask SetupAsync();
    public ValueTask StartAsync();
    public ValueTask StopAsync();
    public ValueTask TearDownAsync();
    
    public EventSubTransport GetCurrentTransport();
    
    public event EventHandler<NotificationEventArgs> OnNotification;
    public event EventHandler<RevocationEventArgs> OnRevocation;
    public event EventHandler<Exception> OnError;
}