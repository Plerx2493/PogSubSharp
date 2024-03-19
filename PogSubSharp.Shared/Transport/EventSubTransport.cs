using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Transport;

public class EventSubTransport
{
    [JsonPropertyName("method")]
    public EventSubMethod Method { get; set; }
    
    [JsonPropertyName("callback"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Callback { get; set; }
    
    [JsonPropertyName("secret"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Secret { get; set; }
    
    [JsonPropertyName("session_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? SessionId { get; set; }
    
    [JsonPropertyName("conduit_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ConduitId { get; set; }
    
    public bool Validate()
    {
        if (Method == EventSubMethod.websocket)
        {
            return 
                !string.IsNullOrWhiteSpace(Callback) 
                && !string.IsNullOrWhiteSpace(Secret)
                && string.IsNullOrWhiteSpace(SessionId)
                && string.IsNullOrWhiteSpace(ConduitId);
                
        }
        else if (Method == EventSubMethod.webhook)
        {
            return 
                string.IsNullOrWhiteSpace(Callback) 
                && string.IsNullOrWhiteSpace(Secret)
                && !string.IsNullOrWhiteSpace(SessionId)
                && string.IsNullOrWhiteSpace(ConduitId);
                
        }
        else if (Method == EventSubMethod.conduit)
        {
            return 
                string.IsNullOrWhiteSpace(Callback) 
                && string.IsNullOrWhiteSpace(Secret)
                && string.IsNullOrWhiteSpace(SessionId)
                && !string.IsNullOrWhiteSpace(ConduitId);
        }
        return false;
    }
}