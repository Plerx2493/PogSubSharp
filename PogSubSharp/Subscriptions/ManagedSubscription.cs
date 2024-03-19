using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Subscriptions;

public struct ManagedSubscription
{
    public EventSubSubscription Subscription { get; set; }
    public string Token { get; set; }
}