using System.Text.Json.Serialization;

namespace PogSubSharp.EventSub.Transport;

public class EventSubFrame
{
    [JsonPropertyName("metadata")]
    public EventSubMessageMetadata Metadata { get; set; }
    
    [JsonPropertyName("payload")]
    public EventSubMessagePayload? Payload { get; set; }
}