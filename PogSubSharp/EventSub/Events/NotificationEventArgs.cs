using PogSubSharp.EventSub.Transport;
using PogSubSharp.Notifications;

namespace PogSubSharp.EventSub.Events;

public class NotificationEventArgs
{
    public EventSubSubscription Subscription { get; set; }
    public IEventSubNotification Event { get; set; }
}