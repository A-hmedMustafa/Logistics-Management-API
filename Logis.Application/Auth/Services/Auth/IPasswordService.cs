using Logis.Application.Auth.Contracts.Auth;
using Microsoft.AspNetCore.Identity;

namespace Logis.Application.Auth.Services.Auth
{
    public interface IPasswordService
    {
        Task<(ChangePasswordStatus Status, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(Guid userId, string oldPassword,string newPassword);
    }
}
