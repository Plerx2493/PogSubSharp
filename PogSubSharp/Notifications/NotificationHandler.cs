using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PogSubSharp.Notifications;

public class NotificationHandler
{
    public delegate Task NotificationHandlerDelegate<T>(T notification) where T : IEventSubNotification;

    private readonly Dictionary<Type, List<Func<IEventSubNotification, Task>>> Handlers =
        new();

    private readonly List<NotificationHandlerDelegate<IEventSubNotification>> CatchEverythingHandlers = [];

    public void RegisterHandler<T>(NotificationHandlerDelegate<T> handler) where T : IEventSubNotification
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
            Handlers[typeof(T)] = new List<Func<IEventSubNotification, Task>> {notification => handler((T) notification)};
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

            // Create a delegate for the static method
            Delegate handler = Delegate.CreateDelegate(typeof(NotificationHandlerDelegate<>).MakeGenericType(genericType), null, staticMethod);
            if (handler is not NotificationHandlerDelegate<IEventSubNotification> notificationDelegate)
            {
                // The fuck happened here?
                continue;
            }
            // If the generic type *is* IEventSubNotification, add it to the catch-all list
            else if (genericType == typeof(IEventSubNotification))
            {
                CatchEverythingHandlers.Add(notificationDelegate);
            }
            // If the generic type *implements* IEventSubNotification, add it to the type specific list
            else
            {
                Func<IEventSubNotification, Task> func = Unsafe.As<Func<IEventSubNotification, Task>>(handler);
                
                if (Handlers.TryGetValue(genericType, out List<Func<IEventSubNotification, Task>>? handlers))
                {
                    handlers.Add(func);
                }
                else
                {
                    Handlers[genericType] = [func];
                }
            }
        }
    }

    public async Task HandleNotificationAsync(IEventSubNotification notification)
    {
        List<Task> tasks = new List<Task>(CatchEverythingHandlers.Count);

        foreach (NotificationHandlerDelegate<IEventSubNotification> handler in CatchEverythingHandlers)
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