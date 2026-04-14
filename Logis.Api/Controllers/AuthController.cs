using Logis.Api.Auth;
using Logis.Application.Auth.Contracts.Auth;
using Logis.Application.Auth.Contracts.Refresh;
using Logis.Application.Auth.Services.Auth;
using Logis.Application.Auth.Services.Security;
using Logis.Application.Auth.Services.Sessions;

using Logis.Domain.Entities;
using Logis.Infrastructure.Auth.Identity;
using Logis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Logis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        
        private readonly ICurrentUser currentUser;
        private readonly ISessionService sessionService;
        private readonly ILoginService loginService;
        private readonly IRefresherService refresherService;
        private readonly IPasswordService passwordService;
        public AuthController(UserManager<AppUser> userManager, ICurrentUser currentUser, ISessionService sessionService, ILoginService loginService, IRefresherService refresherService, IPasswordService passwordService)
        {
            this.userManager = userManager;
            this.currentUser = currentUser;
            this.sessionService = sessionService;
            this.loginService = loginService;
            this.refresherService = refresherService;
            this.passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            var email = (request.Email ?? "").Trim();
            var password = (request.Password ?? "").Trim();


            if (string.IsNullOrWhiteSpace(email))
                return BadRequest();


            if (string.IsNullOrWhiteSpace(password))
                return BadRequest();


            var newUser = new AppUser
            {
                UserName = email,
                Email = email
            };


            var registerResult = await userManager.CreateAsync(newUser, password);
            if (!registerResult.Succeeded)
            {
                var errors = registerResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }


            return Ok(new { message = "User registered successfully." });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // 1) Get The Ip And UserAgent
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            // 2) Login Using Our Service
            var response = await loginService.LoginAsync(request.Email,request.Password, ip, userAgent);
            if (response is null)
                return AuthResults.LoginFailed(this);
        
            // 3) Save The RefreshToken To The Cookies Auto
            Response.Cookies.Append
                (AuthCookie.RefreshTokenName,
                response.RefreshToken,
                AuthCookie.BuildRefreshCookieOptions(Request,response.RefreshTokenExpiresAtUtc));

            return Ok(new LoginResponse { AccessToken = response.AccessToken});
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // 1) Extracting The RawRefreshToken From The Request 
            var rawRefreshToken = Request.Cookies[AuthCookie.RefreshTokenName];
            if (string.IsNullOrWhiteSpace(rawRefreshToken))
                return AuthResults.RefreshNotFound(this);

            // 2) Get The Ip And UserAgent
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();

            // 3) New Refresh Token By The Service We Made
            var response = await refresherService.RefreshAsync(new RefreshRequest(rawRefreshToken,ip,userAgent));

            if(response is null)
                return AuthResults.RefreshNotFound(this);


            // 4) Store The New RefreshToken In The Cookies
            Response.Cookies.Append
                (AuthCookie.RefreshTokenName,
                response.NewRefreshToken,
                AuthCookie.BuildRefreshCookieOptions(Request,response.RefreshTokenExpiresAtUtc));

           // 5) Return The New Access Token
            return Ok(new LoginResponse {AccessToken = response.AcessToken});
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // 1) Get The Raw Refresh Token From The Cookies
            var rawRefershToken = Request.Cookies[AuthCookie.RefreshTokenName];
            
            // 2) Revoke The Active Session Related To This Refresh Token
            await sessionService.LogoutCurrentDeviceAsync(rawRefershToken ?? "");

            // 3) Delete The Refresh Token From The Cookies
            Response.Cookies.Delete(AuthCookie.RefreshTokenName, AuthCookie.DeleteRefreshCookieOptions(Request));

            // 4) Return Nothing
            return NoContent();
        }
        [HttpPost("logout_all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            // 1) Get The Current User Id
            var userId = currentUser.UserId;

            // 2) Validate The Id
            if(userId is null)
                return AuthResults.LoginFailed(this);
          
            // 3) Revoke All Sessions Related To This User
            await sessionService.LogoutAllDevicesAsync(userId.Value);

            // 4) Delete The Refresh Token From The Cookies
            Response.Cookies.Delete(AuthCookie.RefreshTokenName, AuthCookie.DeleteRefreshCookieOptions(Request));

            // 5) Return Nothing
            return NoContent();
        }
        [HttpPost("change_password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = currentUser.UserId;
            if (userId is null)
                return AuthResults.LoginFailed(this);

            var (response,errors) = await passwordService.ChangePasswordAsync(userId.Value,request.CurrentPassword,request.NewPassword);

            if (response == ChangePasswordStatus.InvalidInput)
                return AuthResults.BadInput(this,"Password Are Required");
            if (response == ChangePasswordStatus.Unauthorized)
                return AuthResults.LoginFailed(this);
            if (response == ChangePasswordStatus.IdentityFailed)
                return AuthResults.BadInput(this, errors.ToString());

            Response.Cookies.Delete(AuthCookie.RefreshTokenName, AuthCookie.DeleteRefreshCookieOptions(Request));

            return NoContent();
        }
  
    }
}
