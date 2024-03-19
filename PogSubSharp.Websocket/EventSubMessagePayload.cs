using System.Text.Json.Serialization;
using PogSubSharp.Shared.Notifications.EventSubNotifications;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

[JsonConverter(typeof(EventSocketMessagePayloadConverter))]
public class EventSubMessagePayload
{
    [JsonPropertyName("session")]
    public EventSocketsSession? Session { get; set; }
    
    [JsonPropertyName("subscription")]
    public EventSubSubscription? Subscription { get; set; }
    
    [JsonPropertyName("event")]
    public IEventSubNotification? Event { get; set; }
}