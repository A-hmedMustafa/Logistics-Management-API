using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Services.Sessions
{
    public interface ISessionService
    {
        Task LogoutCurrentDeviceAsync(string rawRefreshToken);
        Task LogoutAllDevicesAsync(Guid userId);
    }
}
