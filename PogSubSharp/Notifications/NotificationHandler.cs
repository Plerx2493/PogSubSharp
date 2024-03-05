using System.Collections.Frozen;
using System.Reflection;

namespace PogSubSharp.Notifications;

public class NotificationHandler
{
    private readonly Dictionary<Type, List<Func<IEventSubNotification, Task>>> Handlers =
        new();

    private FrozenDictionary<Type, List<Func<IEventSubNotification, Task>>>? FrozenHandlers;

    private readonly List<Func<IEventSubNotification, Task>> CatchEverythingHandlers = [];
    
    public bool IsReady => FrozenHandlers is not null;

    public void RegisterHandler<T>(Func<IEventSubNotification, Task> handler) where T : IEventSubNotification
    {
        if (FrozenHandlers is not null)
        {
            throw new InvalidOperationException("NotificationHandler has been frozen");
        }

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
        if (FrozenHandlers is not null)
        {
            throw new InvalidOperationException("NotificationHandler has been frozen");
        }

        foreach (Type type in assembly.GetTypes())
        {
            Type? genericEventSubNotificationHandlerType = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventSubNotificationHandler<>));
            if (genericEventSubNotificationHandlerType is null)
            {
                continue;
            }

            // Grab the IEventSubNotificationHandler<>.HandleNotificationAsync static abstract method
            MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            MethodInfo? staticMethod = null;
            foreach (MethodInfo methode in staticMethods)
            {
                ParameterInfo[] parameters = methode.GetParameters();
                if (
                    methode is {IsStatic: true, IsPublic: true, Name: "HandleNotificationAsync"} &&
                    methode.ReturnType == typeof(Task) &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsAssignableTo(typeof(IEventSubNotification)))
                {
                    staticMethod = methode;
                    break;
                }
            }

            if (staticMethod is null)
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

    /// <summary>
    /// 
    /// </summary>
    public void FreezeHandlers()
    {
        FrozenHandlers = Handlers.ToFrozenDictionary();
    }

    public async Task HandleNotificationAsync(IEventSubNotification notification)
    {
        if (FrozenHandlers is null)
        {
            throw new InvalidOperationException("NotificationHandler has not been frozen");
        }

        List<Task> tasks = new(CatchEverythingHandlers.Count);

        foreach (Func<IEventSubNotification, Task> handler in CatchEverythingHandlers)
        {
            tasks.Add(handler(notification));
        }

        if (FrozenHandlers.TryGetValue(notification.GetType(), out List<Func<IEventSubNotification, Task>>? handlers))
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