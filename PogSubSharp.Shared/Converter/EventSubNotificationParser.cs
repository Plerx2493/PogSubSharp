// auto generated at 04/03/2024 18:03:36 +00:00 

using System.Text.Json;
using PogSubSharp.Shared.Notifications.EventSubNotifications;
using PogSubSharp.Shared.Notifications.EventSubNotifications.Channel;

namespace PogSubSharp.Shared.Converter;

public class EventSubNotificationParser
{

    public static IEventSubNotification Parse(ref Utf8JsonReader reader, JsonSerializerOptions options, string eventType)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Wrong usage of {nameof(EventSubNotificationParser)}");
        }

        IEventSubNotification? notification = eventType switch
        {
            "channel.update" => JsonSerializer.Deserialize<ChannelUpdateEvent>(ref reader, options),
            "channel.follow" => JsonSerializer.Deserialize<ChannelFollowEvent>(ref reader, options),
            "channel.ad_break.begin" => JsonSerializer.Deserialize<ChannelAdBreakBeginEvent>(ref reader, options),
            "channel.chat.clear" => JsonSerializer.Deserialize<ChannelChatClearEvent>(ref reader, options),
            "channel.chat.clear_user_messages" => JsonSerializer.Deserialize<ChannelChatClearUserMessagesEvent>(ref reader, options),
            "channel.chat.message" => JsonSerializer.Deserialize<ChannelChatMessageEvent>(ref reader, options),
            "channel.chat.message_delete" => JsonSerializer.Deserialize<ChannelChatMessageDeleteEvent>(ref reader, options),
            "channel.channel_points_custom_reward_redemption.add" => JsonSerializer.Deserialize<ChannelPointsCustomRewardRedemptionAddEvent>(ref reader, options),

            _ => throw new JsonException("unknown notification type")
        };

        if (notification is null)
        {
            throw new JsonException("Notification could not be deserialized");
        }

        return notification;
    }
}
