using PogSubSharp.Shared.Events;
using PogSubSharp.Shared.Notifications.EventSubNotifications;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    #region Events

    public event EventHandler<IEventSubNotification>? OnNotification;

    protected virtual void OnOnNotification(IEventSubNotification e)
    {
        OnNotification?.Invoke(this, e);
    }
    
    public event EventHandler<RevocationEventArgs>? OnRevocation;
        
    protected virtual void OnOnRevocation(RevocationEventArgs e)
    {
        OnRevocation?.Invoke(this, e);
    }
    
    public event EventHandler<Exception>? OnError;
    
    protected virtual void OnOnError(Exception e)
    {
        OnError?.Invoke(this, e);
    }

    #endregion

    #region HandleTransportEvents
    
    private async Task OnNotificationAsync(object? sender, IEventSubNotification e)
    {
        OnOnNotification(e);
    }
    
    private async Task OnRevocationAsync(object? sender, RevocationEventArgs e)
    {
        OnOnRevocation(e);
    }
    
    private async Task OnErrorAsync(object? sender, Exception e)
    {
        OnOnError(e);
    }

    #endregion
    
}