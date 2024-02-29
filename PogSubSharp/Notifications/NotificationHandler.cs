using System.Collections.Concurrent;
using System.Reflection;

namespace PogSubSharp.Notifications;

public class NotificationHandler
{
    public delegate Task NotificationHandlerDelegate<T>(T notification) where T : IEventSubNotification;

    private readonly ConcurrentDictionary<Type, List<Delegate>> Handlers =
        new();

    private readonly List<NotificationHandlerDelegate<IEventSubNotification>> CatchEverythingHandlers = [];

    public void RegisterHandler<T>(NotificationHandlerDelegate<T> handler) where T : IEventSubNotification
    {
        if (typeof(T) == typeof(IEventSubNotification))
        {
            CatchEverythingHandlers.Add(notification => handler((T) notification));
            return;
        }

        List<Delegate> typeList = Handlers.GetOrAdd(typeof(T), _ => new());
        typeList.Add(handler);
    }

    public void RegisterStaticHandlers(Assembly assembly)
    {
        IEnumerable<Type> handlers = assembly.GetTypes()
            .Where(type => type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubNotificationHandler<>)));


        foreach (Type type in handlers)
        {
            MethodInfo? staticMethod = type.GetMethod("HandleNotificationAsync");
            
            if (staticMethod is null)
            {
                continue;
            }
            
            if (!staticMethod.IsStatic)
            {
                continue;
            }
            
            Type? genericType = type.GetInterfaces().First(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubNotificationHandler<>))
                .GetGenericArguments().FirstOrDefault();

            if (genericType is null)
            {
                continue;
            }

            Delegate handler = Delegate.CreateDelegate(typeof(NotificationHandlerDelegate<>).MakeGenericType(genericType), null, staticMethod);
            
            if (genericType == typeof(IEventSubNotification))
            {
                NotificationHandlerDelegate<IEventSubNotification>? notificationDelegate =
                    (NotificationHandlerDelegate<IEventSubNotification>?)handler;

                if (notificationDelegate is null)
                {
                    continue;
                }
                
                CatchEverythingHandlers.Add(notificationDelegate);
                continue;
            }

            List<Delegate> typeList = Handlers.GetOrAdd(genericType, _ => []);
            typeList.Add(handler);
        }
    }

    public async Task HandleNotificationAsync(IEventSubNotification notification)
    {
        List<Task> tasks = new List<Task>(CatchEverythingHandlers.Count);

        foreach (NotificationHandlerDelegate<IEventSubNotification> handler in CatchEverythingHandlers)
        {
            tasks.Add(handler(notification));
        }

        if (Handlers.TryGetValue(notification.GetType(),
                out List<Delegate>? handlers))
        {
            foreach (Delegate @delegate in handlers)
            {
                tasks.Add((Task)@delegate.DynamicInvoke([notification])!);
            }
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            // handle exceptions here
            // ...
        }
    }
}