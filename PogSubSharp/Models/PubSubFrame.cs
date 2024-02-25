namespace PogSubSharp.Models;

public class PubSubFrame
{
    public PubSubMessageType Type { get; set; }
    public string? Nonce { get; set; }
    public string? Data { get; set; }
    
}