using Logis.Domain.Entities;
using Logis.Infrastructure.Auth.Identity;
using Logis.Infrastructure.Persistence.OutBox;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Persistence
{
    public sealed class AppDbContext : IdentityDbContext<AppUser,IdentityRole<Guid>,Guid>
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        
        }

        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<OutBoxMessage> OutBoxMessages => Set<OutBoxMessage>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}

