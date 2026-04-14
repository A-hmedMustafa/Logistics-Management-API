using Logis.Application.Auth.Services.Auth;
using Logis.Application.Auth.Services.Sessions;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Auth.Sevices.Sessions
{
    public sealed class SessionService : ISessionService
    {
        private readonly AppDbContext db;
        private readonly IRefreshTokenService refreshTokenService;
        public SessionService(AppDbContext db, IRefreshTokenService refreshTokenService)
        {
            this.db = db;
            this.refreshTokenService = refreshTokenService;
        }

        public async Task LogoutAllDevicesAsync(Guid userId)
        {
            // 1) To Avoid Calling It In The Loop
            var now = DateTime.UtcNow;

            // 2) Get All Sessions Belongs To This User
            var sessions = await db.Sessions.Where(s=>s.UserId == userId && s.RevokedAtUtc == null).ToListAsync();

            // 3) Revoke User's Sessions
            foreach ( var session in sessions)
            {
                session.RevokedAtUtc = now;
                session.RevokedReason = "logout_all";
            }
            // 4) Apply Changes In DB
            await db.SaveChangesAsync();
        }

        public async Task LogoutCurrentDeviceAsync(string rawRefreshToken)
        {
            // 1) Validate The Input Refresh Token
            if (string.IsNullOrWhiteSpace(rawRefreshToken))
                return;

            // 2) Hash The Raw Refresh Token To Compare It And Get The Session
            var hashed = refreshTokenService.Hash(rawRefreshToken);
            var activeSession = await db.Sessions.FirstOrDefaultAsync(s=>s.RefreshTokenHash == hashed);

            // 3) Validate The Session
            if (activeSession is null || activeSession.RevokedAtUtc is not null)
                return;

            // 4) Revoke The Current Session -- Mark RevokedReason As "logout"
            activeSession.RevokedAtUtc = DateTime.UtcNow;
            activeSession.RevokedReason = "logout";

            // 5) Applly Changes In DB
            await db.SaveChangesAsync();
        }
    }
}
