using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Transport;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSubMethod
{
    websocket,
    webhook,
    conduit
}