using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public sealed class Notification
    {
       
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public Guid SourceOutboxMessageId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Body { get; private set; } = string.Empty;
        public string? Link { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime? ReadAtUtc {get; private set;  }
        private Notification() { }
        
        public Notification(Guid userId, Guid sourceOutboxMessageId, string title, string body, string? link)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User Id Is Required.");
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title Is Required.");
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body Is Required");

            UserId = userId;
            SourceOutboxMessageId = sourceOutboxMessageId;
            Title = title.Trim();
            Body = body;
            Link = string.IsNullOrWhiteSpace(link) ? null : link.Trim();
            
        }

        public void MarkAsRead() 
        {
            if(ReadAtUtc is not null )
                return;

            ReadAtUtc = DateTime.UtcNow;
        }
    }
}
