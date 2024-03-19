using System.Text.Json.Serialization;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Subscriptions;

public class SubscriptionCreatePayload
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    [JsonPropertyName("condition")]
    public EventSubCondition Condition { get; set; }
    
    [JsonPropertyName("transport")]
    public EventSubTransport Transport { get; set; }
}