namespace Logis.Application.Notifications.Services
{
    public interface IOutBoxEnqueuer
    {
        Task EnqueueAsync(string type, object payload);
    }
}
