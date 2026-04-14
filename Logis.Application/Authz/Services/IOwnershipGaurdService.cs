using Logis.Application.Authz.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Authz.Services
{
    public interface IOwnershipGaurdService
    {
        AuthorizationResult MustBeAuthenticated(Guid? currentUserId);
        AuthorizationResult MustOwn(Guid? currentUserId, Guid resourceOwnerId);
    }
}
