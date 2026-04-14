using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed partial class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> b)
        {
            b.ToTable("Shipments");

            b.HasKey(x => x.Id);

            b.Property(x => x.OrderId).IsRequired();

            b.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            b.Property(x => x.CarrierName)
                .HasMaxLength(200);

            b.Property(x => x.TrackingNumber)
                .HasMaxLength(40)
                .IsRequired();

            b.Property(x => x.TrackingCode)
                .HasMaxLength(20)
                .IsRequired();

            // Typically you want tracking number unique for support/customer calls.
            b.HasIndex(x => x.TrackingNumber).IsUnique();
            b.HasIndex(x => x.TrackingCode).IsUnique();

            // One order -> (usually) one shipment.
            // This enforces the business rule at the DB level.
            b.HasIndex(x => x.OrderId).IsUnique();

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            // ✅ ADD THIS - Tell EF about the backing field
            b.Metadata.FindNavigation(nameof(Shipment.Events))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            // Shipment has many tracking events
            b.HasMany(x => x.Events)
                .WithOne(e => e.Shipment)
                .HasForeignKey(e => e.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
