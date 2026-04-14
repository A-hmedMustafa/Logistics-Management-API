namespace Logis.Api.Contratcs.ShipmentsContracts
{
    public sealed class TrackingEventDto
    {
        public string Type { get; init; } = string.Empty;

        public string? Location { get; init; }

        public string Message { get; init; } = string.Empty;

        public DateTime OccurredAtUtc { get; init; }
    }
}
