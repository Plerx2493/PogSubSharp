using System.Text.Json.Serialization;

namespace PogSubSharp.Models.EventSub;

public class EventSocketsSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; }
    
    [JsonPropertyName("connected_at")]
    public DateTimeOffset ConnectedAt { get; set; }
    
    [JsonPropertyName("keepalive_timeout_seconds")]
    public int? KeepaliveTimeoutSeconds { get; set; }
    
    [JsonPropertyName("reconnect_url")]
    public string? ReconnectUrl { get; set; }
}