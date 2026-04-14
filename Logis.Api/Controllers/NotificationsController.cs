using Logis.Application.Auth.Services.Security;
using Logis.Application.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly INotificationService notificationService;

        public NotificationsController(ICurrentUser currentUser, INotificationService notificationService)
        {
            this.currentUser = currentUser;
            this.notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery]int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unReadOnly = false)
        {
            var currentUserId = currentUser.UserId;
            if(currentUserId is  null) 
                return Unauthorized();

            var allNotis = await notificationService.ListAsync(currentUserId.Value, page, pageSize, unReadOnly); 
            return Ok(allNotis);
        }
        [HttpPost("{shipmentId:guid}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] Guid shipmentId)
        {
            var currentUserId = currentUser.UserId;
            if (currentUserId is null)
                return Unauthorized();

            var IsRead = await notificationService.MarkAsReadAsync(currentUserId.Value, shipmentId);
            if(!IsRead)
                return NotFound();

            return NoContent();
        }
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var currentUserId = currentUser.UserId;
            if (currentUserId is null)
                return Unauthorized();

            var notisCount = await notificationService.MarkAllAsReadAsync(currentUserId.Value);
          
            return Ok( new { Updated = notisCount });
        }
    }
}
