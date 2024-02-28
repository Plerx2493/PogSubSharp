using System.Text.Json.Serialization;

namespace PogSubSharp.Models.EventSub;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSubMethod
{
    websocket,
    webhook
}