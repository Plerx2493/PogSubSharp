using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelAdBreakBeginEvent : IEventSubNotification
{
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
    [JsonPropertyName("requester_user_id")]
    public string RequesterUserId { get; set; }
    
    [JsonPropertyName("requester_user_login")]
    public string RequesterUserLogin { get; set; }
    
    [JsonPropertyName("requester_user_name")]
    public string RequesterrUserName { get; set; }
    
    [JsonPropertyName("duration_seconds")]
    public string DurationSeconds { get; set; }
    
    [JsonPropertyName("started_at")]
    public DateTimeOffset StartedAt { get; set; }
    
    [JsonPropertyName("is_automatic")]
    public bool IsAutomatic { get; set; }
    
}