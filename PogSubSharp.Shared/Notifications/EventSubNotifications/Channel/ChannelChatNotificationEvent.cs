using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelChatNotificationEvent
{
    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }
    
    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }
    
    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }
    
    [JsonPropertyName("chatter_user_id")]
    public string ChatterUserId { get; set; }
    
    [JsonPropertyName("chatter_user_login")]
    public string ChatterUserLogin { get; set; }
    
    [JsonPropertyName("chatter_user_name")]
    public string ChatterUserName { get; set; }
    
    [JsonPropertyName("chatter_is_anonymous")]
    public bool ChatterIsAnonymous { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; }
    
    [JsonPropertyName("badges")]
    public IReadOnlyList<ChatBadge> Badges { get; set; }
    
    [JsonPropertyName("system_message")]
    public string SystemMessage { get; set; }
    
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }
    
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
    
    [JsonPropertyName("notice_type")]
    public ChatNoticeType NoticeType { get; set; }
    
    [JsonPropertyName("sub")]
    public ChatSub Sub { get; set; }
}

public class ChatSub
{
    [JsonPropertyName("duration_months")]
    public int DurationMonths { get; set; }
    
    [JsonPropertyName("sub_tier")]
    public string SubTier { get; set; }
    
    [JsonPropertyName("is_prime")]
    public bool IsPrime { get; set; }
}

public enum ChatNoticeType
{
    sub,
    resub,
    sub_gift,
    community_sub_gift,
    gift_paid_upgrade,
    prime_paid_upgrade,
    raid,
    unraid,
    pay_it_forward,
    announcement,
    bits_badge_tier,
    charity_donation,
}