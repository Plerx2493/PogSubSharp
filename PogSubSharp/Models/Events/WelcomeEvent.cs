using System.Text.Json.Serialization;
using PogSubSharp.Models.EventSub;

namespace PogSubSharp.Models.Events;

public class WelcomeEvent
{
    [JsonPropertyName("metadata")]
    public EventSubMessageMetadata Metadata { get; set; }
    
    [JsonPropertyName("payload")]
    public EventSocketsSession Session { get; set; }
}