using Logis.Application.Auth.Contracts.Auth;
using Logis.Application.Auth.Services.Auth;
using Logis.Application.Auth.Services.Sessions;
using Logis.Infrastructure.Auth.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Auth.Sevices.Auth.AuthServices
{
    public sealed class PasswordService : IPasswordService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ISessionService sessionService;

        public PasswordService(UserManager<AppUser> userManager, ISessionService sessionService)
        {
            this.userManager = userManager;
            this.sessionService = sessionService;
        }

        public async Task<(ChangePasswordStatus Status, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            // 1) oldPassword ??= "";  Same As oldPassword = oldPassword ?? "";
            oldPassword ??= "";
            newPassword ??= "";

            // 2) Validate User Id In Input
            if(userId == Guid.Empty)
                return (ChangePasswordStatus.Unauthorized, null);

            // 3) Validate Passwords In Input
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                return (ChangePasswordStatus.InvalidInput,null);

            // 4) Get The User With This Id 
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user is null)
                return (ChangePasswordStatus.Unauthorized, null);

            // 5) Try To Change The Password
            var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!result.Succeeded)
                return (ChangePasswordStatus.IdentityFailed, result.Errors);

            // 6) Logout All Of The Devices
            await sessionService.LogoutAllDevicesAsync(userId);

            // 7) Return Success
            return (ChangePasswordStatus.Success, null);

        }
    }
}
