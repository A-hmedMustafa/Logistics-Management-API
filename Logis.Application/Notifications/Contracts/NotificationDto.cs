using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Notifications.Contracts
{
    public sealed class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Link { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }
        public bool IsRead => ReadAtUtc is not null;
    }
}
