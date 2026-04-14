namespace Logis.Api.Observability
{
    public interface IDomainEventLogger
    {
        // Orders Events
        void OrderCreated(Guid orderId, Guid userId);
        void OrderConfirmed(Guid orderId, Guid userId);
        void OrderCancelled(Guid orderId, Guid userId, string? reason);

        // Shipments Events
        void ShipmentCreated(Guid shipmentId, Guid orderId ,Guid userId);
        void ShipmentAssigned(Guid shipmentId, Guid orderId, string carrierName ,Guid userId);
        void ShipmentPickedUp(Guid shipmentId, Guid orderId, string? location ,Guid userId);
        void ShipmentInTransit(Guid shipmentId, Guid orderId, string message, string? location, Guid userId);
        void ShipmentDelivered(Guid shipmentId, Guid orderId, string? location, Guid userId);
        void ShipmentCancelled(Guid shipmentId, Guid orderId, string message, Guid userId);
    }
}
