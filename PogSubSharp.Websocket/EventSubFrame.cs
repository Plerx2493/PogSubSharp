using System.Text.Json.Serialization;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

public class EventSubFrame
{
    [JsonPropertyName("metadata")]
    public EventSubMessageMetadata Metadata { get; set; }
    
    [JsonPropertyName("payload")]
    public EventSubMessagePayload? Payload { get; set; }
}