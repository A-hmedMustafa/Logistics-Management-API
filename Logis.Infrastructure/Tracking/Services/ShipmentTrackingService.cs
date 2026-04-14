using Logis.Application.Tracking.Contracts;
using Logis.Application.Tracking.Services;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Tracking.Services
{
    public sealed class ShipmentTrackingService : IShipmentTrackingService
    {
        private readonly AppDbContext db;

        public ShipmentTrackingService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ShipmentTrackingResult?> GetByCodeAsync(string trackingCode)
        {
            var shipmentToTrack = await db.Shipments
                .AsNoTracking()
                .Include(sh=>sh.Events)
                .FirstOrDefaultAsync(sh => sh.TrackingCode == trackingCode);

            if (shipmentToTrack is null)
                return null;
            return new ShipmentTrackingResult
            {
                Status = shipmentToTrack.Status.ToString(),
                CreatedAtUtc = shipmentToTrack.CreatedAtUtc,
                TrackingCode = shipmentToTrack.TrackingCode,
                Events = shipmentToTrack.Events
                .OrderBy(e=>e.OccurredAtUtc)
                .Select(e => new ShipmentTrackingEventResult
                {
                    Location = e.Location,
                    Message = e.Message,
                    OccuredAtUtc = e.OccurredAtUtc,
                    Type = e.EventType.ToString()
                }).ToList()

            };
        }
    }
}
