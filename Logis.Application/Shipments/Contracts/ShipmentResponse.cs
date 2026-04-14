using Logis.Domain.Entities;

namespace Logis.Application.Shipments.Contracts
{
    public sealed class ShipmentResponse
    {
        public required Guid Id { get; init; }
        public required Guid OrderId { get; init; }
        public required ShipmentStatus Status { get; init; }
        public required string TrackingNumber { get; init; }
        public string? CarrierName { get; init; }

        public required DateTime CreatedAtUtc { get; init; }
        public required DateTime UpdatedAtUtc { get; init; }

        public required List<TrackingEventResponse> Events { get; init; }
    }
}
