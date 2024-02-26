using System.Text.Json.Serialization;
using PogSubSharp.Models.Events;

namespace PogSubSharp.Models.EventSub;

public struct EventSubMessagePayload
{
    [JsonPropertyName("session")]
    public EventSocketsSession? Session { get; set; }
    
    [JsonPropertyName("subscription")]
    public EventSubSubscription? Subscription { get; set; }
    
    [JsonPropertyName("event")]
    public IEventSubEvent? Event { get; set; }
}