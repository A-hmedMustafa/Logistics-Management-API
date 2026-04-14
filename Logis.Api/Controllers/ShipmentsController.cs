using Logis.Application.Auth.Services.Security;
using Logis.Infrastructure.Shipments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class ShipmentsController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        private readonly IShipmentService _shipments;

        public ShipmentsController(ICurrentUser currentUser, IShipmentService shipments)
        {
            _currentUser = currentUser;
            _shipments = shipments;
        }

        // Create a shipment for a specific order (Order -> Shipment)
        [HttpPost("{orderId:guid}/shipment")]
        public async Task<IActionResult> CreateForOrder(Guid orderId)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var result = await _shipments.CreateForOrderAsync(_currentUser.UserId.Value, orderId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }

        // Get shipment with tracking events
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var shipment = await _shipments.GetAsync(_currentUser.UserId.Value, id);
            if (shipment is null)
                return NotFound();

            return Ok(shipment);
        }

        // Assign a carrier/driver
        [HttpPost("{id:guid}/assign")]
        public async Task<IActionResult> Assign(Guid id, [FromQuery] string carrierName)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var ok = await _shipments.AssignAsync(_currentUser.UserId.Value, id, carrierName);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // Mark picked up
        [HttpPost("{id:guid}/pickup")]
        public async Task<IActionResult> PickUp(Guid id, [FromQuery] string? location = null)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var ok = await _shipments.MarkPickedUpAsync(_currentUser.UserId.Value, id, location);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // Add transit note (in real life: facility scans, delays, etc.)
        [HttpPost("{id:guid}/transit")]
        public async Task<IActionResult> Transit(Guid id, [FromQuery] string message, [FromQuery] string? location = null)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var ok = await _shipments.AddTransitNoteAsync(_currentUser.UserId.Value, id, message, location);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // Mark delivered
        [HttpPost("{id:guid}/deliver")]
        public async Task<IActionResult> Deliver(Guid id, [FromQuery] string? location = null)
        {
            if (_currentUser.UserId is null)
                return Unauthorized();

            var ok = await _shipments.MarkDeliveredAsync(_currentUser.UserId.Value, id, location);
            if (!ok)
                return NotFound();

            return NoContent();
        }
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelShip(Guid id,[FromQuery] string reason)
        {
            var currentUserId = _currentUser.UserId;
            if(currentUserId is null)
                return Unauthorized();

            var ok = await _shipments.CancelAsync(currentUserId.Value,id, reason);
            if(!ok)
                return NotFound();
            return NoContent();
        }
    }
}
