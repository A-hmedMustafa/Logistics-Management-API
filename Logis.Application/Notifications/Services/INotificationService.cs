using Logis.Application.Notifications.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Notifications.Services
{
    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationDto>> ListAsync(Guid userId, int page, int pageSize, bool unreadOnly);
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
        Task<int> MarkAllAsReadAsync(Guid userId);
    }
}
