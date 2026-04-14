using Logis.Application.Ops.Outbox.Contracts;
using Logis.Application.Ops.Outbox.Services;
using Logis.Infrastructure.Notifications.Policy;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Ops.Outbox
{
    public sealed class OutboxAdminRecoveryService : IOutboxAdminRecoveryService
    {
        private readonly AppDbContext db;

        public OutboxAdminRecoveryService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<bool> RetryAsync(Guid id)
        {
            var dead = await db.OutBoxMessages
                .Where(ob=>ob.Id == id && ob.ProcessedAtUtc == null && ob.IgnoredAtUtc == null)
                .ExecuteUpdateAsync(setters=>setters
                .SetProperty(ob=>ob.Attempts,0)
                .SetProperty(ob=>ob.LastError,(string?) null)
                .SetProperty(ob=>ob.LockedBy,(string?)null)
                .SetProperty(ob=>ob.LockedUntilUtc,(DateTime?)null)
                .SetProperty(ob=>ob.DeadAtUtc,(DateTime?)null));

            return dead == 1;
        }

        public async Task<bool> IgnoreAsync(Guid id)
        {
            var ignored = await db.OutBoxMessages
                .Where(ob=>ob.Id == id && ob.ProcessedAtUtc == null)
                .ExecuteUpdateAsync(setters=>setters
                .SetProperty(ob=>ob.IgnoredAtUtc , DateTime.UtcNow)
                .SetProperty(ob=>ob.LockedBy,(string?)null)
                .SetProperty(ob => ob.LockedUntilUtc, (DateTime?)null));

            return ignored == 1;
        }

        public async Task<int> RetryBatchAsync(OutboxRetryBatchRequest request)
        {
            var mode = (request.Mode ?? "dead").Trim().ToLowerInvariant();
            var max = request.Max <= 0 ? 50 : Math.Min(request.Max, 200);

            var query = db.OutBoxMessages
                .AsNoTracking()
                .Where(ob => ob.ProcessedAtUtc == null && ob.IgnoredAtUtc == null);

            if (mode == "dead")
                query = query.Where(ob => ob.Attempts >= OutboxPolicy.MaxAttempts);

            else if (mode == "failed")
                query = query.Where(ob => ob.Attempts < OutboxPolicy.MaxAttempts && ob.Attempts > 0 && ob.LastError != null);

            else
                throw new InvalidOperationException("Mode Must Be Dead Or Failed.");

            if (!string.IsNullOrWhiteSpace(request.Type))
                query = query.Where(ob => ob.Type == request.Type);

            var messagesIds = await query
                .OrderByDescending(ob => ob.OccurredAtUtc)
                .Select(ob => ob.Id)
                .Take(max)
                .ToListAsync();

            if (messagesIds.Count == 0) return 0;

            var messages = await db.OutBoxMessages
                .Where(ob => messagesIds.Contains(ob.Id) && ob.ProcessedAtUtc == null && ob.IgnoredAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(ob => ob.Attempts, 0)
                .SetProperty(ob => ob.LastError, (string?)null)
                .SetProperty(ob => ob.LockedBy, (string?)null)
                .SetProperty(ob => ob.LockedUntilUtc, (DateTime?)null)
                .SetProperty(ob => ob.DeadAtUtc, (DateTime?)null));

            return messages;
        }
    }
}
