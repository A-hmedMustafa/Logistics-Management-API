namespace Logis.Api.Security.Options
{
    public sealed class OpsAdminOptions
    {
        public const string SectionName = "OpsAdmin";
        public string AdminKey { get; init; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }
}
