namespace PogSubSharp.EventSub.Events;

public class SessionKeepAliveEventArgs
{
    public bool IsHeartbeat { get; set; }
    public bool IsTimeout { get; set; }
}