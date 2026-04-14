using Logis.Application.Ops.Outbox.Contracts;
using Logis.Application.Ops.Outbox.Services;
using Logis.Infrastructure.Notifications.Policy;
using Logis.Infrastructure.Persistence;
using Logis.Infrastructure.Persistence.OutBox;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Ops.Outbox
{
    public sealed class OutboxAdminQueryService : IOutboxAdminQueryService
    {
        private readonly AppDbContext db;

        public OutboxAdminQueryService(AppDbContext db)
        {
            this.db = db;
        }

        public Task<IReadOnlyList<OutboxMessageSummaryDto>> ListFailedAsync(int page, int pageSize, bool includePayload)
        {
            return ListAsync(
                filter: q => q.Where(o=>o.ProcessedAtUtc == null && o.IgnoredAtUtc == null && o.Attempts > 0 && o.Attempts <OutboxPolicy.MaxAttempts && o.LastError != null),
                page, pageSize, includePayload);
        }
        public Task<IReadOnlyList<OutboxMessageSummaryDto>> ListDeadAsync(int page, int pageSize, bool includePayload)
        {
            return ListAsync(
               filter: q => q.Where(o => o.ProcessedAtUtc == null && o.IgnoredAtUtc == null && o.Attempts >= OutboxPolicy.MaxAttempts ),
               page, pageSize, includePayload);
        }
        public Task<IReadOnlyList<OutboxMessageSummaryDto>> ListProcessingAsync(int page, int pageSize, bool includePayload)
        {
            var now = DateTime.UtcNow;
            return ListAsync(
                filter: q => q.Where(o=>o.ProcessedAtUtc == null && o.IgnoredAtUtc == null && o.LockedBy != null && o.LockedUntilUtc != null && o.LockedUntilUtc >= now),
                page,pageSize,includePayload);
        }
        public async Task<OutboxMessageSummaryDto?> GetByIdAsync(Guid id, bool includePayload)
        {
            var outboxMessageDto = await db.OutBoxMessages
                .AsNoTracking()
                .Where(o=>o.Id == id)
                .Select(o=> new OutboxMessageSummaryDto
                {
                    Id = o.Id,
                    Type = o.Type,
                    OccuredAtUtc = o.OccurredAtUtc,
                    ProcessedAtUtc = o.ProcessedAtUtc,
                    Attempts= o.Attempts,
                    DeadAtUtc= o.DeadAtUtc,
                    LastError = o.LastError,
                    LockedBy = o.LockedBy,
                    LockedUntilUtc = o.LockedUntilUtc,
                    Payload = includePayload ? o.Payload : null
                }).FirstOrDefaultAsync();

            if (outboxMessageDto is null) return null;

            outboxMessageDto.Payload = Truncate(outboxMessageDto.Payload, maxLength: includePayload ? 2000 : 0);

            return outboxMessageDto;
        }

        

    

        private async Task<IReadOnlyList<OutboxMessageSummaryDto>> ListAsync(Func<IQueryable<OutBoxMessage> , IQueryable<OutBoxMessage>> filter,
            int page, int pageSize, bool includePayload)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            // query here is actually a query object not just a datatype
            IQueryable<OutBoxMessage> query = db.OutBoxMessages.AsNoTracking();

            query = filter(query);

            query = query.OrderByDescending(x => x.OccurredAtUtc);

            var items = await query
                .Skip((page - 1 ) * pageSize)
                .Take(pageSize).Select(o=> new OutboxMessageSummaryDto
                {
                    Id = o.Id,
                    Type = o.Type,
                    OccuredAtUtc = o.OccurredAtUtc,
                    ProcessedAtUtc = o.ProcessedAtUtc,
                    Attempts = o.Attempts,
                    LockedBy = o.LockedBy,
                    LockedUntilUtc = o.LockedUntilUtc,
                    LastError = o.LastError,
                    DeadAtUtc = o.DeadAtUtc,
                    Payload = includePayload ? o.Payload : null,
                }).ToListAsync();

            foreach(var item in items)
            {
                item.Payload = Truncate(item.Payload, maxLength: includePayload ? 500 : 0);
            }

            return items;
        }
        private static string? Truncate(string? str, int  maxLength)
        {
            if(string.IsNullOrEmpty(str)) return str;
            if(maxLength <= 0) return null;
            return str.Length <= maxLength? str : str.Substring(0, maxLength) + "...";

        }
    }
}
