using System.Text.Json.Serialization;
using PogSubSharp.Converter;
using PogSubSharp.Models.Events;

namespace PogSubSharp.Models.EventSub;

//[JsonConverter(typeof(EventSubMessagePayloadConverter))]
public class EventSubMessagePayload
{
    [JsonPropertyName("session")]
    public EventSocketsSession? Session { get; set; }
    
    [JsonPropertyName("subscription")]
    public EventSubSubscription? Subscription { get; set; }
    
    [JsonPropertyName("event")]
    public IEventSubEvent? Event { get; set; }
}