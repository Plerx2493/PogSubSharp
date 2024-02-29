using System.Text.Json;
using PogSubSharp.Notifications;

namespace PogSubSharp.Converter;


public class EventSubNotificationParser
{
    private static readonly Dictionary<string, Type> lookupTable = new()
    {
        {"channel.channel_points_custom_reward_redemption.add", typeof(ChannelPointsCustomRewardRedemptionAddEvent)}
    };

    public static IEventSubNotification Parse(ref Utf8JsonReader reader, JsonSerializerOptions options, string eventType)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Wrong usage of {nameof(EventSubNotificationParser)}");
        }
        
        if (lookupTable.TryGetValue(eventType, out Type? type))
        {
            throw new JsonException("Not supported event type");
        }

        object? rawNotification = JsonSerializer.Deserialize(ref reader, type!);

        if (rawNotification is null || rawNotification is not IEventSubNotification notification)
        {
            throw new JsonException("Could not parse EventSubNotification");
        }

        return notification;
    }
}