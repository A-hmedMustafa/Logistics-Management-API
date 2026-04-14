using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed partial class ShipmentConfiguration
    {
        public sealed class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
        {
            public void Configure(EntityTypeBuilder<TrackingEvent> b)
            {
                b.ToTable("TrackingEvents");

                b.HasKey(x => x.Id);
                // ✅ ADD THIS - Tell EF that Id is client-generated
                b.Property(x => x.Id)
                    .ValueGeneratedNever();
                b.Property(x => x.ShipmentId).IsRequired();

                b.Property(x => x.EventType)
                    .HasConversion<int>()
                    .IsRequired();

                b.Property(x => x.Location)
                    .HasMaxLength(200);

                b.Property(x => x.Message)
                    .HasMaxLength(500)
                    .IsRequired();

                b.Property(x => x.OccurredAtUtc).IsRequired();

                // Useful query: "give me shipment events ordered by time"
                b.HasIndex(x => new { x.ShipmentId, x.OccurredAtUtc });
            }
        }
    }
}
