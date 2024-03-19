using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelChatMessageDeleteEvent : IEventSubNotification
{
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
    [JsonPropertyName("target_user_id")]
    public string TargetUserId { get; set; }
    
    [JsonPropertyName("target_user_login")]
    public string TargetUserLogin { get; set; }
    
    [JsonPropertyName("target_user_name")]
    public string TargetUserName { get; set; }
    
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }
}