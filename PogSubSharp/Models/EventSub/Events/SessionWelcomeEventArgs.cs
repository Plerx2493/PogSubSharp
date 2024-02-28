namespace PogSubSharp.Models.EventSub.Events;

public class SessionWelcomeEventArgs
{
    public EventSubMessageMetadata Metadata { get; set; }
    public EventSocketsSession Session { get; set; }
}