using System.Text.Json.Serialization;

namespace PogSubSharp.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PubSubMessageType
{
    PING,           // => "PING"
    PONG,           // => "PONG"
    RECONNECT,      // => "RECONNECT"
    AUTH_REVOKED,   // => "AUTH_REVOKED"
    LISTEN,         // => "LISTEN"
    UNLISTEN,       // => "UNLISTEN"
    MESSAGE         // => "MESSAGE"
}