using System.Text.Json.Serialization;

namespace PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

public class ChannelChatMessageEvent : IEventSubNotification
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

    [JsonPropertyName("message_id")]
    public string MessageId { get; set; }

    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("badges")]
    public IReadOnlyList<ChatBadge> Badges { get; set; }

    [JsonPropertyName("message_type")]
    public string MessageType { get; set; }

    [JsonPropertyName("cheer")]
    public string? Cheer { get; set; }

    [JsonPropertyName("reply")]
    public string? Reply { get; set; }

    [JsonPropertyName("channel_points_custom_reward_id")]
    public string? ChannelPointsCustomRewardId { get; set; }
}

public class ChatBadge
{
    [JsonPropertyName("set_id")]
    public string SetId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("info")]
    public string Info { get; set; }
}

public class ChatMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("fragments")]
    public IReadOnlyList<ChatMessageFragment> Fragments { get; set; }
}

public class ChatMessageFragment
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("cheermote")]
    public string? Cheermote { get; set; }

    [JsonPropertyName("emote")]
    public string? Emote { get; set; }

    [JsonPropertyName("mention")]
    public string? Mention { get; set; }
}