using PogSubSharp.EventSub.Transport;

namespace PogSubSharp.EventSub.Events;

public class SessionReconnectEventArgs
{
    public EventSocketsSession Session { get; set; }
}