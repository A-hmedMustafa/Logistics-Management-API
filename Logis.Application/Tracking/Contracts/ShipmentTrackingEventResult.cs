namespace Logis.Application.Tracking.Contracts
{
    public sealed class ShipmentTrackingEventResult
    {
        public string Type { get; init;  } = string.Empty;
        public string? Location { get; init; } 
        public string Message {  get; init; } = string.Empty;
        public DateTime OccuredAtUtc {  get; init; }
    }
}
