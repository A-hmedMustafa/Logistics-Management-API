using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Auth.Identity
{
    public sealed class AppUser: IdentityUser<Guid>
    {
    }
}
