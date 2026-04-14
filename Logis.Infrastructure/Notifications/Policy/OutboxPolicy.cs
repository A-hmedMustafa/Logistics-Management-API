using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Notifications.Policy
{
    public static class OutboxPolicy
    {
        public const int MaxAttempts = 10;
    }
}
