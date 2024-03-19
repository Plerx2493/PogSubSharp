using System.Text.Json.Serialization;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Subscriptions;

public class SubscriptionCreateResponse
{
    [JsonPropertyName("data")]
    public IEnumerable<EventSubSubscription> Data { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("total_cost")]
    public int TotalCost { get; set; }

    [JsonPropertyName("max_total_cost")]
    public int MaxTotalCost { get; set; }

    [JsonIgnore]
    public EventSubSubscription Subscription
    {
        get => Data.First();
        set => Data = new List<EventSubSubscription> {value};
    }
}