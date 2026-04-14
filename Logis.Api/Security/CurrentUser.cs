using Logis.Application.Auth.Services.Security;
using System.Security.Claims;

namespace Logis.Api.Security
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUser(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var user = _contextAccessor.HttpContext?.User;
                if (user is null) 
                    return null;

                var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if(string.IsNullOrWhiteSpace(id))
                    return null;

                return Guid.TryParse(id, out var idGuid) ? idGuid : null;

            }        
        }

        public bool IsAuthenticated => _contextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }
}
