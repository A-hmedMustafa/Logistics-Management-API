using Logis.Application.Auth.Contracts.Refresh;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Services.Auth
{
    public interface IRefreshTokenService
    {
        RefreshTokenResult Create();
        string Hash(string RawToken);
    }
}
