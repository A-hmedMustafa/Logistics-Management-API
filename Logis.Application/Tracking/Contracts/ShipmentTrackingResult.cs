using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Tracking.Contracts
{
    public sealed class ShipmentTrackingResult
    {
        public string TrackingCode { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; } 
        public IReadOnlyList<ShipmentTrackingEventResult> Events { get; init; } = Array.Empty<ShipmentTrackingEventResult>();
    }
}
