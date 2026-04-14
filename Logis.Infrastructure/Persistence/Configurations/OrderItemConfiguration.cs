using Logis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence.Configurations
{
    public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            b.ToTable("OrderItems");
            b.HasKey(oi=>oi.Id);
            b.Property(oi=>oi.OrderId).IsRequired();
            b.Property(oi=>oi.Name).HasMaxLength(250).IsRequired();
            b.Property(oi=>oi.Sku).HasMaxLength(50).IsRequired();
            b.Property(oi=>oi.Quantity).IsRequired();
            b.Property(oi=>oi.UnitPrice).HasPrecision(18,2).IsRequired();
            b.HasIndex(oi=> new {oi.OrderId,oi.Sku});
        }
    }
}
