using Logis.Application.Ops.Outbox.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Ops.Outbox.Services
{
    public interface IOutboxAdminRecoveryService
    {
        Task<bool> RetryAsync(Guid id);
        Task<bool> IgnoreAsync(Guid id);
        Task<int> RetryBatchAsync(OutboxRetryBatchRequest request);
    }
}
