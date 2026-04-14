using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");
            builder.HasKey(s=>s.Id);
            builder.Property(s => s.RefreshTokenHash).IsRequired().HasMaxLength(512);
            builder.Property(s => s.UserAgent).HasMaxLength(512);
            builder.Property(s => s.RevokedReason).HasMaxLength(256);
            builder.Property(s => s.CreatedByIp).HasMaxLength(64);

            builder.HasIndex(s=>s.UserId);
            builder.HasIndex(s=>s.RefreshTokenHash).IsUnique();
            builder.HasIndex(s=>s.RootSessionId);
            builder.HasIndex(s=>s.ParentSessionId);
            builder.HasIndex(s=>s.ReplacedBySessionId);
        }
    }
}
