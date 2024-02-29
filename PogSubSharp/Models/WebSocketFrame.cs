using PogSubSharp.EventSub.Transport;

namespace PogSubSharp.Models;

public struct WebSocketFrame
{
    public EventSubFrame? Frame { get; set; }
    
    public Exception? Exception { get; set; }
    public bool IsClosed { get; set; } 
    public bool IsEmpty { get; set; }
    
    public bool IsException { get; set; }
    
    public static WebSocketFrame Empty = new()
    {
        IsEmpty = true
    };
    
    public static WebSocketFrame Closed = new()
    {
        IsClosed = true
    };

    public static WebSocketFrame FromException(Exception e)
    {
        return new WebSocketFrame()
        {
            Exception = e,
            Frame = null,
            IsClosed = false,
            IsEmpty = false,
            IsException = true
        };
    }
}