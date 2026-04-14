using Logis.Api.Observability;
using Logis.Application.Authz.Contracts;
using Logis.Application.Authz.Services;
using Logis.Application.Notifications.OutboxEvents;
using Logis.Application.Notifications.Services;
using Logis.Application.Shipments.Contracts;
using Logis.Domain.Entities;
using Logis.Infrastructure.Notifications.Services;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Logis.Infrastructure.Shipments.Services
{
    public sealed class ShipmentService : IShipmentService
    {
        private readonly AppDbContext _db;
        private readonly IOwnershipGaurdService _ownership;
        private readonly IDomainEventLogger domainEventLogger;
        private readonly IOutBoxEnqueuer outBoxEnqueuer;
        public ShipmentService(AppDbContext db, IOwnershipGaurdService ownership, IDomainEventLogger domainEventLogger, IOutBoxEnqueuer outBoxEnqueuer)
        {
            _db = db;
            _ownership = ownership;
            this.domainEventLogger = domainEventLogger;
            this.outBoxEnqueuer = outBoxEnqueuer;
        }


        public async Task<CreateShipmentResponse?> CreateForOrderAsync(Guid currentUserId, Guid orderId)
        {
            var orderToShip = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (orderToShip is null)
                return null;

            if (_ownership.MustOwn(currentUserId, orderToShip.UserId) != AuthorizationResult.Allowed)
                return null;

            if (orderToShip.Status != OrderStatus.Confrimed)
                throw new InvalidOperationException("Order Must Be Confirmed To Be Shipped");

            var existingShipment = await _db.Shipments.FirstOrDefaultAsync(sh => sh.OrderId == orderId);
            if (existingShipment is not null)
                return new CreateShipmentResponse
                {
                    ShipmentId = existingShipment.Id,
                    Status = existingShipment.Status,
                    TrackingNumber = existingShipment.TrackingNumber
                };

            var trackingNumber = $"TRK{DateTime.UtcNow:yyyyMMdd}-{GenerateCode(6)}";
            var newShipment = new Shipment(orderId, trackingNumber);
            _db.Shipments.Add(newShipment);
            await _db.SaveChangesAsync();

            domainEventLogger.ShipmentCreated(newShipment.Id, orderId, currentUserId);
            return new CreateShipmentResponse 
            {
                ShipmentId = newShipment.Id,
                Status = newShipment.Status,
                TrackingNumber = newShipment.TrackingNumber 
            };
        }
        public async Task<ShipmentResponse?> GetAsync(Guid currentUserId, Guid shipmentId)
        {
            var shipment = await _db.Shipments
                .Include(sh => sh.Order)
                .Include(sh => sh.Events)
                .FirstOrDefaultAsync(sh => sh.Id == shipmentId);

            if (shipment is null)
                return null;

            if (_ownership.MustOwn(currentUserId, shipment.Order.UserId) != AuthorizationResult.Allowed)
                return null;
            return Map(shipment);
        }
        public async Task<bool> AssignAsync(Guid currentUserId, Guid shipmentId, string carrierName)
        {
            var shipmentToAssign = await LoadOwnedShipment(currentUserId, shipmentId);

            if (shipmentToAssign is null)
                return false;

            shipmentToAssign.AssignCarrier(carrierName);

            await outBoxEnqueuer.EnqueueAsync(OutboxMessageTypes.ShipmentAssigned, 
                new ShipmentAssignedOutboxEvent(
                currentUserId, shipmentToAssign.Id, shipmentToAssign.TrackingCode, carrierName));
            await _db.SaveChangesAsync();
            domainEventLogger.ShipmentAssigned(shipmentToAssign.Id,shipmentToAssign.OrderId, carrierName,currentUserId);
            return true;

        }
        public async Task<bool> MarkPickedUpAsync(Guid currentUserId, Guid shipmentId, string? location)
        {
            var shipmentToPick = await LoadOwnedShipment(currentUserId, shipmentId);
            if (shipmentToPick is null)
                return false;

            shipmentToPick.MarkPickedUp(location);
            await outBoxEnqueuer.EnqueueAsync(OutboxMessageTypes.ShipmentStatusChanged,
                new ShipmentStatusChangedOutboxEvent(
                currentUserId, shipmentToPick.Id, shipmentToPick.TrackingCode, shipmentToPick.Status));
            await _db.SaveChangesAsync();
            domainEventLogger.ShipmentPickedUp(shipmentToPick.Id,shipmentToPick.OrderId, location,currentUserId);
            return true;
        }
        public async Task<bool> AddTransitNoteAsync(Guid currentUserId, Guid shipmentId, string message, string? location)
        {
            var shipmentToTransit = await LoadOwnedShipment(currentUserId, shipmentId);
            if (shipmentToTransit is null)
                return false;

            shipmentToTransit.MarkInTransit(message,location);

            await outBoxEnqueuer.EnqueueAsync(OutboxMessageTypes.ShipmentStatusChanged,
               new ShipmentStatusChangedOutboxEvent(
                currentUserId, shipmentToTransit.Id, shipmentToTransit.TrackingCode, shipmentToTransit.Status));
            await _db.SaveChangesAsync();
            domainEventLogger.ShipmentInTransit(shipmentToTransit.Id, shipmentToTransit.OrderId, message, location, currentUserId);

            return true;
        }
        public async Task<bool> MarkDeliveredAsync(Guid currentUserId, Guid shipmentId, string? location)
        {
            var shipmentToBeDelivered = await LoadOwnedShipment(currentUserId, shipmentId);
            if (shipmentToBeDelivered is null)
                return false;

            shipmentToBeDelivered.MarkDelivered(location);

            await outBoxEnqueuer.EnqueueAsync(OutboxMessageTypes.ShipmentStatusChanged,
                new ShipmentStatusChangedOutboxEvent(
                currentUserId, shipmentToBeDelivered.Id, shipmentToBeDelivered.TrackingCode, shipmentToBeDelivered.Status));

            await _db.SaveChangesAsync();
            domainEventLogger.ShipmentDelivered(shipmentToBeDelivered.Id,shipmentToBeDelivered.OrderId,location, currentUserId);
            return true;
        }

       

        public static string GenerateCode(int len)
        {
            const string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789";
            return new string(Enumerable.Range(0, len)
                .Select(_ => alpha[Random.Shared.Next(alpha.Length)])
                .ToArray());
        }
        private static ShipmentResponse Map(Shipment shipment)
        {
            return new ShipmentResponse
            {
                Id = shipment.Id,
                CarrierName = shipment.CarrierName,
                Status = shipment.Status,
                CreatedAtUtc = shipment.CreatedAtUtc,
                UpdatedAtUtc = shipment.UpdatedAtUtc,
                OrderId = shipment.OrderId,
                TrackingNumber = shipment.TrackingNumber,
                Events = shipment.Events
                .OrderBy(e => e.OccurredAtUtc)
                .Select(e => new TrackingEventResponse
                {
                    EventType = e.EventType,
                    Location = e.Location,
                    Message = e.Message,
                    OccurredAtUtc = e.OccurredAtUtc
                }).ToList()
            };
        }
        private async Task<Shipment?> LoadOwnedShipment(Guid currentUserId, Guid shipmentId)
        {
            var shipment = await _db.Shipments
                .Include(sh => sh.Order)
                .Include(sh => sh.Events)
                .FirstOrDefaultAsync(sh => sh.Id == shipmentId);

            if (shipment is null)
                return null;

            if (_ownership.MustOwn(currentUserId, shipment.Order.UserId) != AuthorizationResult.Allowed)
                return null;

            return shipment;
        }

        public async Task<bool> CancelAsync(Guid currentUserId, Guid shipmentId, string reason)
        {
            var shipmentToCancel = await LoadOwnedShipment(currentUserId, shipmentId);
            if (shipmentToCancel is null)
                return false;

            shipmentToCancel.Cancel(reason);
            
            await _db.SaveChangesAsync();
            domainEventLogger.ShipmentCancelled(shipmentToCancel.Id,shipmentToCancel.Id, reason, currentUserId);
            return true;

        }
    }
}
