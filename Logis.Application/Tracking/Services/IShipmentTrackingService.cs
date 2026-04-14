using Logis.Application.Tracking.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Tracking.Services
{
    public interface IShipmentTrackingService
    {
        Task<ShipmentTrackingResult?> GetByCodeAsync(string trackingCode);
    }
}
