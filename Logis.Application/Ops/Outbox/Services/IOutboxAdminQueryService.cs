using Logis.Application.Ops.Outbox.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Ops.Outbox.Services
{
    public interface IOutboxAdminQueryService
    {
        Task<IReadOnlyList<OutboxMessageSummaryDto>> ListFailedAsync(int page, int pageSize, bool includePayload); 
        Task<IReadOnlyList<OutboxMessageSummaryDto>> ListDeadAsync(int page, int pageSize, bool includePayload); 
        Task<IReadOnlyList<OutboxMessageSummaryDto>> ListProcessingAsync(int page, int pageSize, bool includePayload); 
        Task<OutboxMessageSummaryDto?> GetByIdAsync(Guid id, bool includePayload); 

    }
}
