namespace Logis.Application.Auth.Contracts.Refresh
{
    public sealed record RefreshResponse(string AcessToken, string NewRefreshToken, DateTime RefreshTokenExpiresAtUtc);

}
