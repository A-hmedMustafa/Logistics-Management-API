using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public enum OrderStatus
    {
        Draft = 0,
        Confrimed = 1,
        Cancelled = 2,
        FulfillmentStarted = 3,
        InTransit = 4,
        Delivered = 5
    }
}
