using System.Text;
using System.Text.Json;

namespace Generator;

public class Programm
{
    private const string classStart =
        """
        using System.Text.Json;
        using PogSubSharp.Notifications;
        using PogSubSharp.Notifications.EventSubNotifications.Channel;

        namespace PogSubSharp.Converter;

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
        """;

    private const string classEnd =
        """
                    _ => throw new JsonException("unknown notification type")
                };
        
                if (notification is null)
                {
                    throw new JsonException("Notification could not be deserialized");
                }
    
                return notification;
            }
        }
        """;
    
    public static void Main(String[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("No path provided");
            return;
        }
        
        using StreamReader reader = new(args[0]);

        JsonDocument document = JsonDocument.Parse
        (
            reader.ReadToEnd(),
            options: new()
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }
        );
        IEnumerable<Notification>? notifications = document.Deserialize<IEnumerable<Notification>>();

        if (notifications is null)
        {
            Console.WriteLine("Parsing failed");
            return;
        }

        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine($"// auto generated at {DateTimeOffset.UtcNow} \n");

        sb.AppendLine(classStart);
        
        foreach (Notification notification in notifications)
        {
            sb.AppendLine($"            \"{notification.Id}\" => JsonSerializer.Deserialize<{notification.TypeName}>(ref reader, options),");
        }
        
        sb.AppendLine(classEnd);

        using FileStream fs = File.Create("./EventSubNotificationParser.cs");
        byte[] bytes = new UTF8Encoding(true).GetBytes(sb.ToString());
        fs.Write(bytes, 0, bytes.Length);
    }
}

public class Notification
{
    public string Id { get; set; }
    public string TypeName { get; set; }
}