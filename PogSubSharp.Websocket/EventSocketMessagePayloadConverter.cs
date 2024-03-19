using System.Text.Json;
using System.Text.Json.Serialization;
using PogSubSharp.Shared.Converter;
using PogSubSharp.Shared.Transport;

namespace PogSubSharp.Websocket;

public class EventSocketMessagePayloadConverter : JsonConverter<EventSubMessagePayload>
{
    public override EventSubMessagePayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        EventSubMessagePayload payload = new();
        
        string? eventType = null;
        bool isFirstToken = true;
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && isFirstToken)
            {
                return default;
            }
            
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return payload;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            string? propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "session":
                    payload.Session = JsonSerializer.Deserialize<EventSocketsSession>(ref reader, options);
                    break;
                case "subscription":
                    payload.Subscription = JsonSerializer.Deserialize<EventSubSubscription>(ref reader, options);
                    eventType = payload.Subscription?.Type;
                    break;
                case "event":
                    ArgumentNullException.ThrowIfNull(eventType);
                    try
                    {
                        payload.Event = EventSubNotificationParser.Parse(ref reader, options, eventType);
                    }
                    catch (JsonException)
                    {
                        payload.Event = null;
                    }
                    break;
                default:
                    reader.Skip();
                    break;
            }
            
            isFirstToken = false;
        }
        
        return payload;
    }

    public override void Write(Utf8JsonWriter writer, EventSubMessagePayload value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Session is not null)
        {
            writer.WritePropertyName("session");
            JsonSerializer.Serialize(writer, value.Session, options);
        }

        if (value.Subscription is not null)
        {
            writer.WritePropertyName("subscription");
            JsonSerializer.Serialize(writer, value.Subscription, options);
        }

        if (value.Event is not null)
        {
            writer.WritePropertyName("event");
            JsonSerializer.Serialize(writer, value.Event, options);
        }

        writer.WriteEndObject();
    }
}