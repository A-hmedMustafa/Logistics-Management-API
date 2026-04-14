namespace Logis.Application.Auth.Options
{
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public List<JwtKeyOptions> SigningKeys { get; init; } = new ();
        public int AccessTokenMinutes { get; init; } = 15;
    }
}
