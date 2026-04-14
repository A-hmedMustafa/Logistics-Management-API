using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> b)
        {
            b.ToTable("Notifications");
            b.HasKey(n => n.Id);

            b.Property(n => n.UserId).IsRequired();
            b.Property(n => n.Body).HasMaxLength(2000).IsRequired();
            b.Property(n => n.Title).HasMaxLength(200).IsRequired();
            b.Property(n => n.Link).HasMaxLength(500);
            b.Property(n => n.SourceOutboxMessageId).IsRequired();

            b.HasIndex(n => n.SourceOutboxMessageId).IsUnique();
            b.HasIndex(n => new { n.UserId, n.CreatedAtUtc });
            b.HasIndex(n => new { n.UserId, n.ReadAtUtc });
        }
    }
}
