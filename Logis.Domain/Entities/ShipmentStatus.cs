using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public enum ShipmentStatus
    {
        Created = 0,     // Shipment exists but not yet assigned to a carrier/driver
        Assigned = 1,    // Carrier assigned (driver/company)
        PickedUp = 2,    // Package collected from pickup address
        InTransit = 3,   // Moving / scanned in transit
        Delivered = 4,   // Delivered to recipient
        Cancelled = 5    // Cancelled before delivery
    }
}