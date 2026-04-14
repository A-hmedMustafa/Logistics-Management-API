using Microsoft.AspNetCore.Mvc;

namespace Logis.Api.Auth
{
    public static class AuthResults
    {
        public static IActionResult RefreshNotFound(ControllerBase c) => c.NotFound();
        public static IActionResult LoginFailed(ControllerBase c) => c.Unauthorized();
        public static IActionResult BadInput(ControllerBase c, string message) => c.BadRequest(message);  
    }
}
