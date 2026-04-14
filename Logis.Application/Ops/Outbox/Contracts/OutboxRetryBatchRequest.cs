namespace Logis.Application.Ops.Outbox.Contracts
{
    public sealed class OutboxRetryBatchRequest
    {
        public string Mode { get; set; } = "dead";
        public string? Type { get; set; }
        public int Max { get; set; } = 50;
    }
}
