using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Transport;

public class EventSubCondition
{
    [JsonPropertyName("broadcaster_user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BroadcasterId { get; set; }
    
    [JsonPropertyName("user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? UserId { get; set; }
    
    [JsonPropertyName("moderator_user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ModeratorUserId { get; set; }

    [JsonPropertyName("from_broadcaster_user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? FromBroadcasterUserId { get; set; }

    [JsonPropertyName("to_broadcaster_user_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ToBroadcasterUserId { get; set; }

    [JsonPropertyName("reward_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? RewardId { get; set; }
}