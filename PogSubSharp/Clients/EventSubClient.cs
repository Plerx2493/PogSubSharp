using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    private bool isCanceled = false;
    private EventSubWebSocket _webSocket;
    private ILogger _logger;
    
    public EventSubClient(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger<EventSubClient>();
        _webSocket = new EventSubWebSocket(_logger);
    }
}