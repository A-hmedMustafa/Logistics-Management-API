namespace Logis.Application.Auth.Options
{
    public sealed class JwtKeyOptions
    {
        public string Kid { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
    }
}
