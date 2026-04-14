using Logis.Application.Tracking.Contracts;

namespace Logis.Api.Contratcs.ShipmentsContracts
{
    public sealed  class PublicShipmentTrackingDto
    {
        public string TrackingCode { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public IReadOnlyList<ShipmentTrackingEventResult> Events { get; init; } = Array.Empty<ShipmentTrackingEventResult>();
    }
}
