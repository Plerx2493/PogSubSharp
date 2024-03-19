using PogSubSharp.Clients;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Subscriptions;

public class SubscriptionManager
{
    private readonly EventSubClient _client;
    
    private List<ManagedSubscription> _subscriptions = [];

    public int SubscriptionsCost { get; private set; }
    public int SubscriptionsCount { get; private set; }
    public int MaxCost { get; private set; }
    
    public SubscriptionManager(EventSubClient client)
    {
        _client = client;
    }
    
    public async Task ShutdownAsync()
    {
        foreach (ManagedSubscription mSubscription in _subscriptions)
        {
            await _client.DeleteSubscriptionAsync(mSubscription.Subscription.Id, mSubscription.Token);
        }
    }

    public async Task<SubscriptionCreateResponse> SubscribeToChannelUpdate(string broadcasterId, string oauthToken)
    {
        SubscriptionCreatePayload? payload = new()
        {
            Type = "channel.update",
            Version = "2",
            Condition = new EventSubCondition
            {
                BroadcasterUserId = broadcasterId
            },
            Transport = new EventSubTransport
            {
                Method = EventSubMethod.websocket,
                SessionId = _client.SessionId
            }
        };

        SubscriptionCreateResponse res = await _client.CreateSubscriptionAsync(payload, oauthToken);
        
        _subscriptions.AddRange(res.Data.Select(
            x => new ManagedSubscription
            {
                Subscription = x,
                Token = oauthToken
            }));
        
        SubscriptionsCount++;
        SubscriptionsCost += res.Subscription.Cost;
        MaxCost = res.MaxTotalCost;
        
        return res;   
    }
}