using Logis.Domain.Entities;

namespace Logis.Application.Shipments.Contracts
{
    public sealed class TrackingEventResponse
    {
        public required TrackingEventType EventType { get; init; }
        public required string Message { get; init; }
        public string? Location { get; init; }
        public required DateTime OccurredAtUtc { get; init; }
    }
}
