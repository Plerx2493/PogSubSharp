using System.Text.Json.Serialization;

namespace PogSubSharp.EventSub.Transport;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSubMethod
{
    websocket,
    webhook
}