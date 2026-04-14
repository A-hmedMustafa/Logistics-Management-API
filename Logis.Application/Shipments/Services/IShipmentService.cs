using Logis.Application.Shipments.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Shipments.Services
{
    public interface IShipmentService
    {
        Task<CreateShipmentResponse?> CreateForOrderAsync(Guid currentUserId, Guid orderId);

        Task<ShipmentResponse?> GetAsync(Guid currentUserId, Guid shipmentId);

        Task<bool> AssignAsync(Guid currentUserId, Guid shipmentId, string carrierName);

        Task<bool> MarkPickedUpAsync(Guid currentUserId, Guid shipmentId, string? location);

        Task<bool> AddTransitNoteAsync(Guid currentUserId, Guid shipmentId, string message, string? location);
        
        Task<bool> MarkDeliveredAsync(Guid currentUserId, Guid shipmentId, string? location);
        
        Task<bool> CancelAsync(Guid currentUserId, Guid shipmentId,string reason);
    }
}
