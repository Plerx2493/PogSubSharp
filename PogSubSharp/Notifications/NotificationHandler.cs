using System.Reflection;

namespace PogSubSharp.Notifications;

public class NotificationHandler
{

    private readonly Dictionary<Type, List<Func<IEventSubNotification, Task>>> Handlers =
        new();

    private readonly List<Func<IEventSubNotification, Task>> CatchEverythingHandlers = [];

    public void RegisterHandler<T>(Func<IEventSubNotification, Task> handler) where T : IEventSubNotification
    {
        if (typeof(T) == typeof(IEventSubNotification))
        {
            CatchEverythingHandlers.Add(notification => handler((T) notification));
            return;
        }

        if (Handlers.TryGetValue(typeof(T), out List<Func<IEventSubNotification, Task>>? handlers))
        {
            handlers.Add(notification => handler((T) notification));
        }
        else
        {
            Handlers[typeof(T)] = [notification => handler((T) notification)];
        }
    }
    

    public void RegisterStaticHandlers(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            Type? genericEventSubNotificationHandlerType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubNotificationHandler<>));
            if (genericEventSubNotificationHandlerType is null)
            {
                continue;
            }
            
            // Grab the IEventSubNotificationHandler<>.HandleNotificationAsync static abstract method
            MethodInfo? staticMethod = type.GetMethod("HandleNotificationAsync");
            if (staticMethod is null || !staticMethod.IsStatic)
            {
                continue;
            }

            // Grab the T in IEventSubNotificationHandler<T>
            Type genericType = genericEventSubNotificationHandlerType.GetGenericArguments()[0];

            // Create a func for the static method
            Func<IEventSubNotification, Task> handler = (x) => (Task) staticMethod.Invoke(null, [x])!;
            
            // If the generic type *is* IEventSubNotification, add it to the catch-all list
            if (genericType == typeof(IEventSubNotification))
            {
                CatchEverythingHandlers.Add(handler);
            }
            // If the generic type *implements* IEventSubNotification, add it to the type specific list
            else
            {
                if (Handlers.TryGetValue(genericType, out List<Func<IEventSubNotification, Task>>? handlers))
                {
                    handlers.Add(handler);
                }
                else
                {
                    Handlers[genericType] = [handler];
                }
            }
        }
    }

    public async Task HandleNotificationAsync(IEventSubNotification notification)
    {
        List<Task> tasks = new List<Task>(CatchEverythingHandlers.Count);

        foreach (Func<IEventSubNotification, Task> handler in CatchEverythingHandlers)
        {
            tasks.Add(handler(notification));
        }
        
        if (Handlers.TryGetValue(notification.GetType(), out List<Func<IEventSubNotification, Task>>? handlers))
        {
            foreach (Func<IEventSubNotification, Task> handler in handlers)
            {
                tasks.Add(handler(notification));
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