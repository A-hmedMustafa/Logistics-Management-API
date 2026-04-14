using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Authz.Contracts
{
    public enum AuthorizationResult
    {
        Allowed = 0,
        Forbidden = 1,
        UnAuthenticated = 2
    }
}
