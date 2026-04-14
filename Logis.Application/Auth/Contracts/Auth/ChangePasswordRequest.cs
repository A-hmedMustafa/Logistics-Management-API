namespace Logis.Application.Auth.Contracts.Auth
{
    public sealed class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword {  get; set; } = string.Empty;
    }
}
