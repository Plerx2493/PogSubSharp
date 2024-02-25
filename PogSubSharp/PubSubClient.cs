using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PogSubSharp.Models;

namespace PogSubSharp;

public class PubSubClient
{
    private ClientWebSocket _client;
    
    private readonly byte[] _receiveRawBuffer = new byte[4096];
    private readonly byte[] _sendRawBuffer = new byte[4096];
    
    private readonly MemoryStream _receiveStream = new();
    private readonly MemoryStream _sendStream = new();

    private readonly CancellationToken _ct;
    
    public PubSubClient( CancellationToken ct = default)
    {
        _client = new ClientWebSocket();
        _ct = ct;
    }
    
    public async Task ConnectAsync(Uri? uri = null)
    {
        uri ??= new Uri("wss://pubsub-edge.twitch.tv");
        await _client.ConnectAsync(uri, _ct);
    }
    
    public async Task SendAsync(string message)
    {
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        await _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
    
    public async Task<PubSubFrame> ReceiveAsync()
    {
        if (_client.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not open");
        }
        
        _receiveStream.SetLength(0);
        
        WebSocketReceiveResult? result;
        
        do
        {
            result = await _client.ReceiveAsync(_receiveRawBuffer, CancellationToken.None);
            _receiveStream.Write(_receiveRawBuffer, 0, result.Count);
        } while (!result.EndOfMessage);
        
        _receiveStream.Seek(0, SeekOrigin.Begin);
        
        
            
        return JsonSerializer.Deserialize<PubSubFrame>(_receiveStream);
        
        
    }
    
}