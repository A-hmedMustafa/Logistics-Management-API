using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.ToTable("Orders");
            b.HasKey(o=>o.Id);
            b.Property(o => o.UserId).IsRequired();
            b.Property(o => o.Status).HasConversion<int>().IsRequired();
            b.Property(o => o.OrderNumber).HasMaxLength(32).IsRequired();
            b.Property(o => o.WeightKg).HasPrecision(18,2).IsRequired();
            b.Property(o => o.DeclaredValue).HasPrecision(18,2).IsRequired();
            b.Property(o => o.CurrencyCode).HasMaxLength(3).IsRequired();
            b.Property(o => o.CreatedAtUtc).IsRequired();
            b.Property(o => o.UpdatedAtUtc).IsRequired();
            b.Property(o => o.Notes).HasMaxLength(1000);
            b.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.OwnsOne(o => o.DeliveryAddress, owned =>
            {
                owned.Property(a => a.ContactName).HasMaxLength(150).IsRequired();
                owned.Property(a => a.Phone).HasMaxLength(30).IsRequired();
                owned.Property(a => a.Line1).HasMaxLength(250).IsRequired();
                owned.Property(a => a.Line2).HasMaxLength(250);
                owned.Property(a => a.City).HasMaxLength(120).IsRequired();
                owned.Property(a => a.State).HasMaxLength(120);
                owned.Property(a => a.PostalCode).HasMaxLength(30).IsRequired();
                owned.Property(a => a.CountryCode).HasMaxLength(2).IsRequired();
            });

            b.OwnsOne(o => o.PickupAddress, owned =>
            {
                owned.Property(a => a.ContactName).HasMaxLength(150).IsRequired();
                owned.Property(a => a.Phone).HasMaxLength(30).IsRequired();
                owned.Property(a => a.Line1).HasMaxLength(250).IsRequired();
                owned.Property(a => a.Line2).HasMaxLength(250);
                owned.Property(a => a.City).HasMaxLength(120).IsRequired();
                owned.Property(a => a.State).HasMaxLength(120);
                owned.Property(a => a.PostalCode).HasMaxLength(30).IsRequired();
                owned.Property(a => a.CountryCode).HasMaxLength(2).IsRequired();
            });
            b.HasIndex(o => o.UserId);
            b.HasIndex(o => o.OrderNumber).IsUnique();
            b.HasIndex(o => o.Status);

            
        }
    }
}
