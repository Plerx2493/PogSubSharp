using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelFollowEvent : IEventSubNotification
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; }
    
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }
    
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
    [JsonPropertyName("followed_at")]
    public DateTimeOffset FollowedAt { get; set; }
}