using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Notifications.OutboxEvents
{
    public sealed record OrderCreatedOutBoxEvent(Guid UserId, Guid OrderId, string OrderNumber);
}
