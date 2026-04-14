using Logis.Api.Security;
using Logis.Application.Ops.Outbox.Contracts;
using Logis.Application.Ops.Outbox.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Controllers
{
    [OpsAdminKey]
    [Route("api/[controller]")]
    [ApiController]
    public class OutboxAdminController : ControllerBase
    {
        private readonly IOutboxAdminQueryService outboxAdmin;
        private readonly IOutboxAdminRecoveryService recoveryService;

        public OutboxAdminController(IOutboxAdminQueryService outboxAdmin, IOutboxAdminRecoveryService recoveryService)
        {
            this.outboxAdmin = outboxAdmin;
            this.recoveryService = recoveryService;
        }


        [HttpGet("failed")]
        public async Task<IActionResult> Failed([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includePayload = false)
        {
            var items = await outboxAdmin.ListFailedAsync(page, pageSize, includePayload);
            return Ok(items);
        }
        [HttpGet("dead")]
        public async Task<IActionResult> Dead([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includePayload = false)
        {
            var items = await outboxAdmin.ListDeadAsync(page, pageSize, includePayload);
            return Ok(items);
        }
        [HttpGet("processing")]
        public async Task<IActionResult> Processing([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includePayload = false)
        {
            var items = await outboxAdmin.ListProcessingAsync(page, pageSize, includePayload);
            return Ok(items);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id, [FromQuery] bool includePayload = false)
        {
            var item = await outboxAdmin.GetByIdAsync(id,includePayload);
            if(item is null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost("{id:guid}/retry")]
        public async Task<IActionResult> Retry([FromRoute] Guid id)
        {
            var ok = await recoveryService.RetryAsync(id);
            if(!ok)
                return NotFound(new {error = "either notfound or already deleted"});
            return NoContent();
        }

        [HttpPost("{id:guid}/ignore")]
        public async Task<IActionResult> Ignore([FromRoute] Guid id)
        {
            var ignored = await recoveryService.IgnoreAsync(id);
            if (!ignored)
                return NotFound();
            return NoContent();
        }


        [HttpPost("retry-batch")]
        public async Task<IActionResult> RetryBatch([FromBody] OutboxRetryBatchRequest request)
        {
            var batchCount = await recoveryService.RetryBatchAsync(request);
            return Ok(new { batchCount });
        }
    }
}
