using System.Collections;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PogSubSharp.Clients;
using PogSubSharp.Notifications;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PogSubSharp.Test;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();
        
        ILogger<EventSubClient> microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<EventSubClient>();
        
       EventSubClient client = new EventSubClient(microsoftLogger);
       
       Test test = new Test();
       Test2 test2 = new Test2();
       
       client.NotificationHandler.RegisterHandler<IEventSubNotification>(test.DoSomethingAsync);
       client.NotificationHandler.RegisterHandler<ChannelPointsCustomRewardRedemptionAddEvent>(test2.DoSomethingAsync);
       client.NotificationHandler.RegisterHandler<ChannelPointsCustomRewardRedemptionAddEvent>(test.DoSomethingAsync);
       client.NotificationHandler.RegisterStaticHandlers(Assembly.GetExecutingAssembly());
       
       await client.ConnectAsync("ws://127.0.0.1:8080/ws");
       await Task.Delay(-1);
    }
}

class Test : IEventSubNotificationHandler<IEventSubNotification>
{
    string name = "test";
    
    public static Task HandleNotificationAsync(IEventSubNotification notification)
    {
        Console.WriteLine("Global handler");
        return Task.CompletedTask;
    }
    
    public Task DoSomethingAsync(IEventSubNotification notification)
    {
        Console.WriteLine($"Global handler in class {name}");
        return Task.CompletedTask;
    }
}

class Test2 : IEventSubNotificationHandler<ChannelPointsCustomRewardRedemptionAddEvent>
{
    string name = "test2";
    
    public static Task HandleNotificationAsync(ChannelPointsCustomRewardRedemptionAddEvent notification)
    {
        Console.WriteLine("Channel update handler");
        return Task.CompletedTask;
    }
    
    public Task DoSomethingAsync(IEventSubNotification notification)
    {
        Console.WriteLine($"Global handler in class {name}");
        return Task.CompletedTask;
    }
}
