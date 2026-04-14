namespace Logis.Domain.Entities
{
    public sealed class Shipment
    {
        // A ShipmentId Is For The Db
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid OrderId { get; private set; }
        public Order Order { get; private set; } = null!;

        public ShipmentStatus Status { get; private set; } = ShipmentStatus.Created;
        public string? CarrierName { get; private set; }

        // Tracking Number Is For Invoices, Emails, Reporting, etc...
        public string TrackingNumber { get; private set; } = string.Empty;
        
        // Tracking Code Is For The Customer To Track His Order 
        public string TrackingCode { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get;private set;  } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get;private set; } = DateTime.UtcNow;

        private readonly List<TrackingEvent> _events = new();
        public IReadOnlyCollection<TrackingEvent> Events => _events;
        private Shipment() { }

        public Shipment(Guid orderId,string trackingNumber)
        {
            if(orderId == Guid.Empty)
                throw new ArgumentException("OrderId Is Required",nameof(orderId));

            if(string.IsNullOrWhiteSpace(trackingNumber))
                throw new ArgumentException("Tracking Number Is Required",nameof(trackingNumber));

            OrderId = orderId;
            TrackingNumber = trackingNumber.Trim();
            TrackingCode = GenerateTrackingCode();
            AddEvent(TrackingEventType.Created, "Shipment Created.");
            Touch();
        }

        public static string GenerateTrackingCode()
        {
            return $"TRK-{Guid.NewGuid():N}".Substring(0, 16).ToUpper();
        }
        private void AddEvent(TrackingEventType eventType, string message, string? location = null)
        {
            _events.Add(new TrackingEvent(this.Id, eventType, message, location));  // ✅ Pass this.Id
        }
        private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

        // Domain Rules
        public void AssignCarrier(string carrierName)
        {
            if(string.IsNullOrWhiteSpace(carrierName))
                throw new ArgumentException("Carrier Name Is Required", nameof(carrierName));

            if(Status == ShipmentStatus.Delivered)
                throw new InvalidOperationException("Cannot assign a delivered shipment.");
            if (Status == ShipmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot assign a Cancelled shipment.");

            CarrierName = carrierName.Trim();
            Status = ShipmentStatus.Assigned;

            AddEvent(TrackingEventType.Assigned, $"Carrier assigned: {CarrierName}");
            Touch();
        }
        public void MarkPickedUp(string? location)
        {
           
            if (Status != ShipmentStatus.Assigned && Status != ShipmentStatus.Created)
                throw new InvalidOperationException("Pickup is allowed only after creation/assignment.");


           
            Status = ShipmentStatus.PickedUp;

            AddEvent(TrackingEventType.PickedUp, "Shipment PickedUp.",location);
            Touch();
        }
        public void MarkInTransit(string message, string? location)
        {

            if (Status != ShipmentStatus.PickedUp && Status != ShipmentStatus.InTransit)
                throw new InvalidOperationException("InTransit allowed only after pickup.");



            Status = ShipmentStatus.InTransit;

            AddEvent(TrackingEventType.Note, message, location);
            Touch();
        }
        public void MarkDelivered(string? location)
        {
            if (Status != ShipmentStatus.InTransit && Status != ShipmentStatus.PickedUp)
                throw new InvalidOperationException("Delivery allowed only after pickup/in-transit.");

            Status = ShipmentStatus.Delivered;
            AddEvent(TrackingEventType.Delivered, "Shipment delivered.", location);
            Touch();
        }
        public void Cancel(string reason)
        {
            if (Status == ShipmentStatus.Delivered)
                throw new InvalidOperationException("Delivered shipments cannot be cancelled.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Cancellation reason is required.", nameof(reason));

            Status = ShipmentStatus.Cancelled;
            AddEvent(TrackingEventType.Cancelled, $"Shipment cancelled: {reason.Trim()}");
            Touch();
        }
    }
}