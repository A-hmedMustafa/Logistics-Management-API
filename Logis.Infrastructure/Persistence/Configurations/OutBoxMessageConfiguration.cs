using Logis.Infrastructure.Persistence.OutBox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed class OutBoxMessageConfiguration : IEntityTypeConfiguration<OutBoxMessage>
    {
        public void Configure(EntityTypeBuilder<OutBoxMessage> b)
        {
            b.ToTable("OutBoxMessages");
            b.HasKey(ob => ob.Id);

            b.Property(ob => ob.Type).HasMaxLength(200).IsRequired();
            b.Property(ob => ob.Payload).HasMaxLength(8000).IsRequired();
            b.Property(ob => ob.LockedBy).HasMaxLength(100);
            b.Property(ob => ob.LastError).HasMaxLength(1000);
            b.Property(ob => ob.DeadAtUtc);
            b.Property(ob => ob.IgnoredAtUtc);
            

            b.HasIndex(ob => ob.OccurredAtUtc);
            b.HasIndex(ob => ob.DeadAtUtc);     // Ops queries: "show me dead messages" fast
            b.HasIndex(ob => ob.ProcessedAtUtc);
            b.HasIndex(ob => ob.LockedUntilUtc);
            b.HasIndex(ob => ob.IgnoredAtUtc);
        }
    }
}
