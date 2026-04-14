using Logis.Application.Auth.Services.Security;
using Logis.Application.Orders.Contracts;
using Logis.Application.Orders.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly IOrderService orderService;

        public OrdersController(IOrderService orderService, ICurrentUser currentUser)
        {
            this.orderService = orderService;
            this.currentUser = currentUser;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if(currentUser.UserId is null)
                return Unauthorized();

            var response = await orderService.CreateAsync(currentUser.UserId.Value, request);
            return CreatedAtAction(nameof(GetById),new {id =  response.Id},response);
        }
        [HttpPost("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            if(currentUser.UserId is null)
                return Unauthorized();

            var ok = await orderService.ConfirmAsync(currentUser.UserId.Value, id);
            if(!ok)
                return NotFound();
            return Ok(ok);
        }
        [HttpGet("getById/{id:guid}")]
        public async Task<IActionResult> GetById(Guid orderId)
        {
            if(currentUser.UserId is null)
                return Unauthorized();

            var order = await orderService.GetAsync(currentUser.UserId.Value, orderId);
            if(order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet("getall")]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if(currentUser.UserId is null)
                return Unauthorized();

            var orders = await orderService.ListAsync(currentUser.UserId.Value, page, pageSize);
            if(orders is null)
                return NotFound();

            return Ok(orders);
        }

        [HttpPost("{orderId:guid}/cancel")]
        public async Task<IActionResult> Cancel( Guid orderId, [FromQuery] string? reason = null)
            {
            if(currentUser.UserId is null)
                return Unauthorized();

            var Cancelled = await orderService.CancelAsync(currentUser.UserId.Value,orderId, reason);
            if(!Cancelled)
                return NotFound();

            return NoContent();
        }
    }
}
