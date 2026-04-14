using Logis.Application.Auth.Contracts.Refresh;
using Logis.Application.Auth.Services.Auth;
using Logis.Domain.Entities;
using Logis.Infrastructure.Auth.Identity;
using Logis.Infrastructure.Auth.Sevices.Auth.JwtService;
using Logis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Auth.Sevices.Auth.RefreshTokens
{
    public sealed class RefesherService : IRefresherService
    {
        private readonly AppDbContext db;
        private readonly JwtTokenService tokenService;
        private readonly IRefreshTokenService refreshTokenService;
        private readonly UserManager<AppUser> userManager;

        public RefesherService(AppDbContext db, JwtTokenService tokenService, IRefreshTokenService refreshTokenService, UserManager<AppUser> userManager)
        {
            this.db = db;
            this.tokenService = tokenService;
            this.refreshTokenService = refreshTokenService;
            this.userManager = userManager;
        }

        public async Task<RefreshResponse?> RefreshAsync(RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RawRefreshToken))
                return null;

            var hashed = refreshTokenService.Hash(request.RawRefreshToken);
                
            await using var tx = await db.Database.BeginTransactionAsync();
            var session = await db.Sessions.FirstOrDefaultAsync(s=>s.RefreshTokenHash == hashed);
            
            if (session is null) 
                return null;

            if(session.ReplacedBySessionId != null)
            {
                await RevokeSessionFamilyAsync(session.RootSessionId, "replay_detected");
                await tx.CommitAsync();
                return null;
            }

            if (session.RevokedAtUtc != null)
                return null;
            if(session.ExpiresAtUtc <= DateTime.UtcNow)
                return null;

            var refreshTokenObject = refreshTokenService.Create();

            var childSession = new Session
            {
                Id = Guid.NewGuid(),
                UserId = session.UserId,
                RefreshTokenHash = refreshTokenObject.TokenHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = refreshTokenObject.ExpiresAtUtc,

                RootSessionId = session.RootSessionId,
                ParentSessionId = session.Id,
                ReplacedBySessionId = null,

                CreatedByIp = request.Ip,
                UserAgent = request.UserAgent
            };

            session.RevokedAtUtc = DateTime.UtcNow;
            session.ReplacedBySessionId = childSession.Id;
            session.RevokedReason = "rotated";

            db.Sessions.Add(childSession);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            var user = await userManager.FindByIdAsync(session.UserId.ToString());
            if (user is null)
                return null;

            var accessToken = tokenService.CreateAccessToken(user);

            return new RefreshResponse(AcessToken: accessToken,NewRefreshToken:refreshTokenObject.RawToken,refreshTokenObject.ExpiresAtUtc);

        }

        private async Task RevokeSessionFamilyAsync(Guid rootSessionId, string reason)
        {
            var now = DateTime.UtcNow;

            var sessions = await db.Sessions.Where(s => s.RootSessionId == rootSessionId && s.RevokedAtUtc == null).ToListAsync();

            foreach ( var session in sessions)
            {
                session.RevokedAtUtc = now;
                session.RevokedReason = reason;
            }

            await db.SaveChangesAsync();
        }
    }
}
