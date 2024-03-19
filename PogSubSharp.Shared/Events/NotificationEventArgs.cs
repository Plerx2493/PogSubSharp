using PogSubSharp.Shared.Notifications.EventSubNotifications;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Shared.Events;

public class NotificationEventArgs
{
    public EventSubSubscription Subscription { get; set; }
    public IEventSubNotification Event { get; set; }
}