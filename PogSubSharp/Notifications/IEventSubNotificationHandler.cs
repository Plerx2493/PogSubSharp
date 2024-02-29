namespace PogSubSharp.Notifications;

public interface IEventSubNotificationHandler<T> where T : IEventSubNotification
{
    public static abstract Task HandleNotificationAsync(T notification);
}