using PogSubSharp.Models.EventSub;

namespace PogSubSharp.Models;

public struct WebSocketFrame
{
    public EventSubFrame? Frame { get; set; }
    public bool IsClosed { get; set; } 
    public bool IsEmpty { get; set; }
    
    public static WebSocketFrame Empty = new()
    {
        IsEmpty = true
    };
    
    public static WebSocketFrame Closed = new()
    {
        IsClosed = true
    };
}