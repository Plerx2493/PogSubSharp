using System.Text.Json.Serialization;

namespace PogSubSharp.EventSub.Transport;

public class EventSubSubscription
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    [JsonPropertyName("cost")]
    public int Cost { get; set; }
    
    [JsonPropertyName("condition")]
    public Dictionary<string,string> Condition { get; set; }
    
    [JsonPropertyName("transport")]
    public EventSubTransport Transport { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}