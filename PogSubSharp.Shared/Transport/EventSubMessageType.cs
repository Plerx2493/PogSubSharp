using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Transport;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSubMessageType
{
    session_welcome,
    session_keepalive,
    notification,
    session_reconnect,
    revocation
}