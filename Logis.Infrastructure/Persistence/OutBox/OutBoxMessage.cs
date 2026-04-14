using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.OutBox
{
    // OutboxMessage represents a guaranteed system event that must be processed later.
 //
 // Purpose:
 // - Ensures important actions (emails, notifications, integrations, etc.)
 //   are NOT lost if the application crashes or external services fail.
 // - Stored in the database together with business data in the same transaction.
 // - Processed asynchronously by a background worker.
 // - Retries automatically until successful.
 //
 // In simple words:
 // "Write down what must be done later, before trying to do it."
    public sealed class OutBoxMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;
        public string Type { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public int Attempts { get; set; }
        public string? LastError { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }

        // Locking for multi-instance processing
        public DateTime? LockedUntilUtc { get; set; }

        // When Attempts hit MaxAttempts and message became "dead"
        public DateTime? DeadAtUtc { get; set; }

        // Ops: manually quarantined; excluded from retries/lists by default
        public DateTime? IgnoredAtUtc { get; set; }
        public string? LockedBy { get; set; }
        private OutBoxMessage() { }
        public OutBoxMessage(string type, string payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type Cannot Be Null");

            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentException("Payload Cannot Be Null");

            Type = type.Trim();
            Payload = payload;
        }

        public void ClearLock() 
        {
            LockedBy = null;
            LockedUntilUtc = null;
        }
    }
}
