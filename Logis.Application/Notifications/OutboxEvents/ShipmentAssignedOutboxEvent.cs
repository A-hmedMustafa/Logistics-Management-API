namespace Logis.Application.Notifications.OutboxEvents
{
    public sealed record ShipmentAssignedOutboxEvent(Guid UserId, Guid ShipmentId, string TrackingCode, string CarrierName);
}
