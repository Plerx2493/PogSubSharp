using System.Text.Json;
using PogSubSharp.Models.Events;

namespace PogSubSharp.Converter;

public class EventSubEventParser 
{
    public static IEventSubEvent Parse(ref Utf8JsonReader reader, JsonSerializerOptions options, string eventType)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        IEventSubEvent? eventSubEvent = eventType switch
        {
            "channel.channel_points_custom_reward_redemption.add" => JsonSerializer.Deserialize<ChannelPointsCustomRewardRedemptionAddEvent>(ref reader, options),
            _ => throw new JsonException("Not supported event type")
        };

        return eventSubEvent;
    }
}