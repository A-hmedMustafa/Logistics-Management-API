namespace Logis.Domain.Entities
{
    public sealed class TrackingEvent
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ShipmentId { get; private set; }
        public Shipment Shipment { get; private set; } = null!;
        public TrackingEventType EventType { get; private set; }
        public string? Location { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

        private TrackingEvent() { }

        // ✅ ADD shipmentId parameter here
        public TrackingEvent(Guid shipmentId, TrackingEventType eventType, string message, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Tracking Message Is Required.", nameof(message));

            ShipmentId = shipmentId;  // ✅ Set it in constructor
            EventType = eventType;
            Message = message.Trim();
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
            OccurredAtUtc = DateTime.UtcNow;
        }
    }
}