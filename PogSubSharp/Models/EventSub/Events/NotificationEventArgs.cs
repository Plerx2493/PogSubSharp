using PogSubSharp.Models.Events;

namespace PogSubSharp.Models.EventSub.Events;

public class NotificationEventArgs
{
    public EventSubSubscription Subscription { get; set; }
    public IEventSubEvent Event { get; set; }
}