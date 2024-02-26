using System.Text.Json.Serialization;

namespace PogSubSharp.Models.EventSub;

public class EventSubTransport
{
    [JsonPropertyName("method")]
    public EventSubMethod Method { get; set; }
    
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }
    
    [JsonPropertyName("connected_at")]
    public DateTimeOffset ConnectedAt { get; set; }
    
    [JsonPropertyName("disconnected_at")]
    public DateTimeOffset? DisconnectedAt { get; set; }
}