using Logis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Shipments.Contracts
{
    public sealed class CreateShipmentResponse
    {
        public required Guid ShipmentId { get; init; }
        public required string TrackingNumber { get; init; }
        public required ShipmentStatus Status { get; init; }
    }
}
