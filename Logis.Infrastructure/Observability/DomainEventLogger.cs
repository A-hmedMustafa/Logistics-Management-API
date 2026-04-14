using Microsoft.Extensions.Logging;

namespace Logis.Api.Observability
{
    public sealed class DomainEventLogger: IDomainEventLogger
    {
        private readonly ILogger<DomainEventLogger> _logger;

        public DomainEventLogger(ILogger<DomainEventLogger> logger)
        {
            _logger = logger;
        }

        // Orders Events
        public void OrderCreated(Guid orderId, Guid userId)
        {
            _logger.LogInformation("DomainEvent OrderCreated orderId={OrderId} userId={UserId}",
                orderId,userId);
        }
        public void OrderConfirmed(Guid orderId, Guid userId)
        {
            _logger.LogInformation("DomainEvent OrderConfirmed orderId={OrderId} userId={UserId}",
                orderId, userId);
        }
        public void OrderCancelled(Guid orderId, Guid userId, string? reason)
        {
            _logger.LogInformation("DomainEvent OrderCancelled orderId={OrderId} userId={UserId} reason={Reason}", 
                orderId, userId, reason);
        }

        // Shipments Events
        public void ShipmentCreated(Guid shipmentId, Guid orderId, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentCreated shipmentId={ShipmentId} orderId={OrderId} userId={UserId}",
                shipmentId, orderId, userId);
        }
        public void ShipmentAssigned(Guid shipmentId, Guid orderId, string carrierName, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentAssigned shipmentId={ShipmentId} orderId={OrderId} carrierName={CarrierName} userId={UserId}",
                 shipmentId, orderId,carrierName, userId);
        }
        public void ShipmentPickedUp(Guid shipmentId, Guid orderId, string? location, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentPickedUp shipmentId={ShipmentId} orderId={OrderId} location={Location} userId={UserId}",
                 shipmentId, orderId, location, userId);
        }
        public void ShipmentDelivered(Guid shipmentId, Guid orderId, string? location, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentDelivered shipmentId={ShipmentId} orderId={OrderId} location={Location} userId={UserId}",
                 shipmentId, orderId, location, userId);
        }
        public void ShipmentInTransit(Guid shipmentId, Guid orderId, string message, string? location, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentDelivered shipmentId={ShipmentId} orderId={OrderId} location={Location} userId={UserId}",
                shipmentId, orderId, location, userId);
        }
        public void ShipmentCancelled(Guid shipmentId, Guid orderId, string reason, Guid userId)
        {
            _logger.LogInformation("DomainEvent ShipmentDelivered shipmentId={ShipmentId} orderId={OrderId} reason={Reason} userId={UserId}",
                shipmentId, orderId, reason, userId);
        }
    }
}
