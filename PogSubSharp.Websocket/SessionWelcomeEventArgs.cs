using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

public class SessionWelcomeEventArgs
{
    public EventSubMessageMetadata Metadata { get; set; }
    public EventSocketsSession Session { get; set; }
}