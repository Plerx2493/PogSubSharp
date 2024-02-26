using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PogSubSharp.Models.EventSub;

namespace PogSubSharp;

public class EventSubWebSocket
{
    private ClientWebSocket _client;
    private readonly byte[] _receiveRawBuffer = new byte[4096];
    private readonly MemoryStream _receiveStream = new();
    private readonly SemaphoreSlim _receiveLock = new(1, 1);
    private readonly CancellationToken _ct;
    private readonly ILogger<EventSubWebSocket> _logger;
    private int _keepAliveInterval;
    
    public EventSubWebSocket(ILogger<EventSubWebSocket> logger, CancellationToken ct = default, int keepAliveInterval = 30)
    {
        _logger = logger;
        _ct = ct;
        _keepAliveInterval = keepAliveInterval;
        _client = new ClientWebSocket();
    }
    
    public async Task ConnectAsync(Uri? uri = null)
    {
        var builder = new UriBuilder(uri ?? new Uri("wss://eventsub.wss.twitch.tv/ws"));
        builder.Query = $"keepalive_timeout_seconds={_keepAliveInterval}";
        await _client.ConnectAsync(builder.Uri, _ct);
    }
    
    public async Task DisconnectAsync()
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", _ct);
    }
    
    public async Task<EventSubFrame> ReceiveAsync()
    {
        if (_client.State == WebSocketState.Closed)
        {
            _logger.LogDebug("WebSocket is closed");
            _logger.LogDebug("Closed with close status: {CloseStatus}", _client.CloseStatus);
            throw new InvalidOperationException("WebSocket is closed");
        }
        
        await _receiveLock.WaitAsync(_ct);
        
        _receiveStream.SetLength(0);

        try
        {
            WebSocketReceiveResult? result;
            do
            {
                result = await _client.ReceiveAsync(_receiveRawBuffer, CancellationToken.None);
                _receiveStream.Write(_receiveRawBuffer, 0, result.Count);
            } while (!result.EndOfMessage);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        
        _receiveStream.Seek(0, SeekOrigin.Begin);
        var frame = JsonSerializer.Deserialize<EventSubFrame>(_receiveStream)!;
        
        _receiveLock.Release();

        return frame;
    }
}