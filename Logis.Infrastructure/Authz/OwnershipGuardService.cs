using Logis.Application.Authz.Contracts;
using Logis.Application.Authz.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Authz
{
    public sealed class OwnershipGuardService : IOwnershipGaurdService
    {
        public AuthorizationResult MustBeAuthenticated(Guid? currentUserId) 
            => currentUserId is null ? AuthorizationResult.UnAuthenticated : AuthorizationResult.Allowed;
        

        public AuthorizationResult MustOwn(Guid? currentUserId, Guid resourceOwnerId)
        {
            if (currentUserId is null)
                return AuthorizationResult.UnAuthenticated;

            return currentUserId == resourceOwnerId
                ? AuthorizationResult.Allowed
                : AuthorizationResult.Forbidden;
        }
    }
}
