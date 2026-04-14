namespace Logis.Application.Auth.Contracts.Auth
{
    public sealed record LoginServiceResponse(string AccessToken,string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
}
