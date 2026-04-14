using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Contracts.Refresh
{
    public sealed record RefreshRequest(string RawRefreshToken, string? Ip, string? UserAgent);
}
