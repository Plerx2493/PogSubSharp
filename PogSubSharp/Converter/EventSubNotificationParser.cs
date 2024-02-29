using System.Text.Json;
using PogSubSharp.Notifications;

namespace PogSubSharp.Converter;

public class EventSubNotificationParser
{

    public static IEventSubNotification Parse(ref Utf8JsonReader reader, JsonSerializerOptions options, string eventType)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Wrong usage of {{nameof(EventSubNotificationParser)}}");
        }

        IEventSubNotification? notification = eventType switch
        {
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