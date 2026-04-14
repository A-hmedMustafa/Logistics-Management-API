using Logis.Application.Auth.Contracts.Refresh;
using Logis.Application.Auth.Options;
using Logis.Application.Auth.Services.Auth;
using Logis.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace Logis.Infrastructure.Auth.Sevices.Auth.RefreshTokens
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly RefreshTokenOptions tokenOptions;

        public RefreshTokenService(IOptions<RefreshTokenOptions> tokenOptions)
        {
            this.tokenOptions = tokenOptions.Value;
            if (string.IsNullOrWhiteSpace(this.tokenOptions.Pepper))
                throw new InvalidOperationException("RefreshToken:Pepper Is Missing");
           
        }

        public RefreshTokenResult Create()
        {
            var rawToken = GenerateSecretToken();
            var hashedToken = Hash(rawToken);
            var expiresAtUtc = DateTime.UtcNow.AddDays(tokenOptions.LifeTimeDays);

            return new RefreshTokenResult(rawToken,hashedToken,expiresAtUtc);
        }
        public string Hash(string RawToken)
        {
            
            if(string.IsNullOrWhiteSpace(RawToken))
                throw new ArgumentException("Token Cannot Be Empty" , nameof(RawToken));

            var input = $"{RawToken}.{tokenOptions.Pepper}";
            var tokenBytes = Encoding.UTF8.GetBytes(input);
            var hashed = SHA256.HashData(tokenBytes);
            return Convert.ToBase64String(hashed);
        }
        private static string GenerateSecretToken()
        {
            var Bytes = new byte[64];
            RandomNumberGenerator.Fill(Bytes);
            return Convert.ToBase64String(Bytes);
        }
       
    }
}
