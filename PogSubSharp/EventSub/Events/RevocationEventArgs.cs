using PogSubSharp.EventSub.Transport;

namespace PogSubSharp.EventSub.Events;

public class RevocationEventArgs
{
    public EventSubSubscription Subscription { get; set; }
}