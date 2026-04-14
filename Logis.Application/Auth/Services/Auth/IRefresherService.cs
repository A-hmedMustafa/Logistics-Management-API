using Logis.Application.Auth.Contracts.Refresh;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Services.Auth
{
    public interface IRefresherService
    {
        Task<RefreshResponse?> RefreshAsync(RefreshRequest request); 
    }
}
