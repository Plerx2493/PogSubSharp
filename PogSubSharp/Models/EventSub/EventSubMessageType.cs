using System.Text.Json.Serialization;

namespace PogSubSharp.Models.EventSub;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSubMessageType
{
    session_welcome,
    session_keepalive,
    notification,
    session_reconnect,
    revocation
}