using Logis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Logis.Domain.Entities;
using Logis.Application.Auth.Contracts.Auth;
using Logis.Application.Auth.Services.Auth;
using Logis.Infrastructure.Auth.Identity;
using Logis.Infrastructure.Auth.Sevices.Auth.JwtService;
namespace Logis.Infrastructure.Auth.Sevices.Auth.AuthServices
{
    public sealed class LoginService : ILoginService
    { 
        private readonly UserManager<AppUser> userManager;
        private readonly JwtTokenService tokenService;
        private readonly IRefreshTokenService refreshTokenService;
        private readonly AppDbContext db;

        public LoginService(UserManager<AppUser> userManager, JwtTokenService tokenService, IRefreshTokenService refreshTokenService, AppDbContext db)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.refreshTokenService = refreshTokenService;
            this.db = db;
        }

        public async Task<LoginServiceResponse?> LoginAsync(string email, string password, string? ip, string? userAgent)
        {
            // 1) Fix Input Issuess If There Is Issues
            email = (email ?? "").Trim();
            password = password ?? "";
            // 2) Validate Input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;
            // 3) Get The User That Has This Email Address
            var userToLogin = await userManager.FindByEmailAsync(email);
            // 4) Validate The "userToLogin"
            if (userToLogin is null)
                return null;
            // 5) Check If Password Mathches
            var loginResult = await userManager.CheckPasswordAsync(userToLogin, password);
            if(!loginResult)
                return null;

            // 6) Generate New Access And Refresh Tokens
            var accessToken = tokenService.CreateAccessToken(userToLogin);
            var refreshTokenObject = refreshTokenService.Create();

            // 7) Generate New Session With The New Refresh Token
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userToLogin.Id,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = refreshTokenObject.ExpiresAtUtc,
                RefreshTokenHash = refreshTokenObject.TokenHash,

                RootSessionId = Guid.Empty,
                ParentSessionId = null,
                ReplacedBySessionId = null,

                CreatedByIp = ip,
                UserAgent = userAgent
            };
            // 8) Mark This Session As The First Of Its Family
            session.RootSessionId = session.Id;
            // 9) Add The Session To Db And Apply Changes
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            // 10) Return Tokens And Refresh Token LifeTime
            return new LoginServiceResponse(accessToken, refreshTokenObject.RawToken, refreshTokenObject.ExpiresAtUtc);
        }
    }
}
