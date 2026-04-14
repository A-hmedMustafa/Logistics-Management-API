using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public sealed class Session
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshTokenHash { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedReason { get; set; }
        public Guid RootSessionId { get; set; }
        public Guid? ParentSessionId { get; set; }
        public Guid? ReplacedBySessionId { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public bool IsRevoked => RevokedAtUtc != null;
    }
}
