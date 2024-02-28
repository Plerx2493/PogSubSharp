using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PogSubSharp.Clients;
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
        
        var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<EventSubClient>();
        
       var client = new EventSubClient(microsoftLogger);
       await client.ConnectAsync("ws://127.0.0.1:8080/ws");
       await Task.Delay(-1);
    }
}