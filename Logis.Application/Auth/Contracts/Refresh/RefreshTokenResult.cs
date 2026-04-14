namespace Logis.Application.Auth.Contracts.Refresh
{
    public sealed record RefreshTokenResult(string RawToken, string TokenHash, DateTime ExpiresAtUtc);
}
