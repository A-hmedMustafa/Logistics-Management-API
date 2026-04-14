using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public sealed class Payment
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid OrderId { get; private set; }
        public string PaymentIntentId { get; private set; } = string.Empty;
        public long AmountInCents { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.pending;
        public string? FailureReason { get; set; }
        public string? Cardlast4 { get; set; }
        public string? CardBrand { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAtUtc { get; set; }
        public Order Order { get; private set; } = null!;


        private Payment() { }
        public Payment(Guid orderId, string paymentIntentId, long amountInCents, string currency)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("Order Id Is Required", nameof(orderId));
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new ArgumentException("Payment Intent Id Is Required", nameof(paymentIntentId));
            if (amountInCents <= 0)
                throw new ArgumentOutOfRangeException("Amount Must Be Positive", nameof(amountInCents));
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new ArgumentException("Currency Must Be A 3-Letter ISO Code (e.g., USD, EGP)", nameof(currency));

            OrderId = orderId;
            PaymentIntentId = paymentIntentId;
            AmountInCents = amountInCents;
            Currency = currency.ToUpperInvariant();
        }

        public void MarkAsSuccessed(string cardLast4, string cardBrand)
        {
            if (Status == PaymentStatus.Succeeded) return;

            Status = PaymentStatus.Succeeded;
            UpdatedAtUtc = DateTime.UtcNow;
            PaidAtUtc = DateTime.UtcNow;
            Cardlast4 = cardLast4;
            CardBrand = cardBrand;
        }

        public void MarkAsFailed(string reason)
        {
            Status = PaymentStatus.Failed;
            FailureReason = reason;
            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void MarkAsRequireAction()
        {
            Status = PaymentStatus.RequiresAction;
            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void MarkAsProcessing()
        {
            Status = PaymentStatus.Processing;
            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void MarkAsCancelled()
        {
            if (Status == PaymentStatus.Succeeded)
                throw new InvalidOperationException("Cannot Cancel A Succeeded Payment. Use Refund Instead");

            Status = PaymentStatus.Cancelled;
            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void MarkAsRefunded()
        {
            if (Status != PaymentStatus.Succeeded)
                throw new InvalidOperationException("Can Only Refund Succeded Payments");

            Status = PaymentStatus.Refunded;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public enum PaymentStatus
    {
        pending = 0,
        RequiresAction = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6
    }
}
