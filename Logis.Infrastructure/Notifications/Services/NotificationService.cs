using Logis.Application.Notifications.Contracts;
using Logis.Application.Notifications.Services;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Notifications.Services
{
    public sealed class NotificationService : INotificationService
    {
        private readonly AppDbContext db;

        public NotificationService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IReadOnlyList<NotificationDto>> ListAsync(Guid userId, int page, int pageSize, bool unreadOnly)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
            
            var allNotis =  db.Notifications.AsNoTracking().Where(n=>n.UserId == userId);
            if (unreadOnly)
                allNotis = allNotis.Where(n => n.ReadAtUtc == null);

            var unreadNotis = await allNotis                
                .OrderByDescending(no=>no.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(no => new NotificationDto
                {
                    Id = no.Id,
                    Title = no.Title,
                    Body = no.Body,
                    Link = no.Link,
                    CreatedAtUtc = no.CreatedAtUtc,
                    ReadAtUtc= no.ReadAtUtc
                })
                .ToListAsync();

            

            return unreadNotis;   
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var allUnreadNotis = await db.Notifications.Where(n=>n.UserId == userId && n.ReadAtUtc == null).ToListAsync();

            foreach (var n in allUnreadNotis)
                n.MarkAsRead();

            await db.SaveChangesAsync();
            return allUnreadNotis.Count;
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notiToRead = await db.Notifications.FirstOrDefaultAsync(n=>n.UserId==userId && n.Id == notificationId);
            if(notiToRead == null)
                return false;

            notiToRead.MarkAsRead();
            await db.SaveChangesAsync();
            return true;
        }
    }
}
