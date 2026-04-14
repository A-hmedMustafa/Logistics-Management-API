using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Auth.Options
{
    public sealed class RefreshTokenOptions
    {
        public int LifeTimeDays { get; init; } = 30;
        public string Pepper { get; init; } = string.Empty;
    }
}
