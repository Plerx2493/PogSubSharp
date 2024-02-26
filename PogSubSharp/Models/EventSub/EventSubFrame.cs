using System.Text.Json;
using System.Text.Json.Serialization;

namespace PogSubSharp.Models.EventSub;

public struct EventSubFrame
{
    [JsonPropertyName("metadata")]
    public EventSubMessageMetadata Metadata { get; set; }
    
    [JsonPropertyName("payload")]
    public EventSubMessagePayload Payload { get; set; }
}