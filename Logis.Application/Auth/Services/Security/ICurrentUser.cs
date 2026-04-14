using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Services.Security
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
