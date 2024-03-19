using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Shared.Events;

public class RevocationEventArgs
{
    public EventSubSubscription Subscription { get; set; }
}