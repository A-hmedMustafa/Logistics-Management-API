using Logis.Api.Contratcs.ShipmentsContracts;
using Logis.Application.Tracking.Contracts;
using Logis.Application.Tracking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly IShipmentTrackingService shipmentTracking;

        public TrackingController(IShipmentTrackingService shipmentTracking)
        {
            this.shipmentTracking = shipmentTracking;
        }

        [HttpGet("{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await shipmentTracking.GetByCodeAsync(code);

            if (result is null)
                return NotFound(new { message = "Invalid Tracking Code" });

            var response = new PublicShipmentTrackingDto
            {
                TrackingCode = result.TrackingCode,
                Status = result.Status,
                CreatedAtUtc = result.CreatedAtUtc,
                Events = result.Events.Select(e => new ShipmentTrackingEventResult
                {
                    Type = e.Type,
                    Location = e.Location,
                    Message = e.Message,
                    OccuredAtUtc = e.OccuredAtUtc
                }).ToList()
            };
            return Ok(response);
        }
    }
}
