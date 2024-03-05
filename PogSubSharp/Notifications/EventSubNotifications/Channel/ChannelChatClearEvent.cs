using System.Text.Json.Serialization;

namespace PogSubSharp.Notifications.EventSubNotifications.Channel;

public class ChannelChatClearEvent : IEventSubNotification
{
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
}