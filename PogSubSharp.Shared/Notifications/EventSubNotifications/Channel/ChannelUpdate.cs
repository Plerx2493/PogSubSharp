using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelUpdateEvent : IEventSubNotification
{
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("language")]
    public string Language { get; set; }
    
    [JsonPropertyName("category_id")]
    public string CategoryId { get; set; }
    
    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; }
    
    [JsonPropertyName("content_classification_labels")]
    public IReadOnlyList<string> ContentClassificationLabels { get; set; }
}