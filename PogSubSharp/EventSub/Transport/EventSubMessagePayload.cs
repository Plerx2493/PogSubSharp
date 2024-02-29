using System.Text.Json.Serialization;
using PogSubSharp.Converter;
using PogSubSharp.Notifications;

namespace PogSubSharp.EventSub.Transport;

[JsonConverter(typeof(EventSubMessagePayloadConverter))]
public class EventSubMessagePayload
{
    [JsonPropertyName("session")]
    public EventSocketsSession? Session { get; set; }
    
    [JsonPropertyName("subscription")]
    public EventSubSubscription? Subscription { get; set; }
    
    [JsonPropertyName("event")]
    public IEventSubNotification? Event { get; set; }
}