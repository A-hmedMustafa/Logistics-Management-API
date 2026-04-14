using Logis.Application.Auth.Contracts.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Services.Auth
{
    public interface ILoginService
    {
        Task<LoginServiceResponse?> LoginAsync(string email, string password, string? ip, string? userAgent);
    }
}
