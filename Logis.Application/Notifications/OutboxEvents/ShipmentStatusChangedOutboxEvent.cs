using Logis.Domain.Entities;

namespace Logis.Application.Notifications.OutboxEvents
{
    public sealed record ShipmentStatusChangedOutboxEvent(Guid UserId, Guid ShipmentId, string TrackingCode, ShipmentStatus NewStatus);
}
