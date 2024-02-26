using Microsoft.Extensions.Logging.Abstractions;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    private bool isCanceled = false;
    private EventSubWebSocket _webSocket;
    
    public EventSubClient()
    {
        _webSocket = new EventSubWebSocket(new NullLogger<EventSubWebSocket>());
    }
}