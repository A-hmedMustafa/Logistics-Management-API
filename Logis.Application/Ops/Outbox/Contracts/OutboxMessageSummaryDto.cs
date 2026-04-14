using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Ops.Outbox.Contracts
{
    public class OutboxMessageSummaryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;

        public DateTime OccuredAtUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
        public DateTime? DeadAtUtc { get; set; }

        public int Attempts { get; set; }
        public string? LastError { get; set; }

        public string? LockedBy { get; set; }
        public DateTime? LockedUntilUtc { get; set; }

        public string? Payload { get; set; }
    }
    public sealed class OutboxMessageDetailsDto: OutboxMessageSummaryDto
    {

    }
}
