using System.Text.Json.Serialization;

namespace PogSubSharp.EventSub.Transport;

public class EventSubMessageMetadata
{
    [JsonPropertyName("message_id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("message_type")]
    public EventSubMessageType Type { get; set; }
    
    [JsonPropertyName("message_timestamp")]
    public DateTimeOffset Timestamp { get; set; }
    
    [JsonPropertyName("subscription_type")]
    public string? SubscriptionType { get; set; }
    
    [JsonPropertyName("subscription_version")]
    public string? SubscriptionVersion { get; set; }
}