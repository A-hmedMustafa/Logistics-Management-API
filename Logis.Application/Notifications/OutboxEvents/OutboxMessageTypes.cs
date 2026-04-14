using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Notifications.OutboxEvents
{
    public static class OutboxMessageTypes
    {
        public const string OrderCreated = "OrderCreated";
        public const string ShipmentAssigned = "ShipmentAssigned";
        public const string ShipmentStatusChanged = "ShipmentStatusChanged";
    }
}
