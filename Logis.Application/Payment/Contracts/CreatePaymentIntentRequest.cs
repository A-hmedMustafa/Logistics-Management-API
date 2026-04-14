using Logis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Payment.Contracts
{
    public sealed class CreatePaymentIntentRequest
    {
        public Guid OrderId { get; set; }
    }
    public sealed class ConfirmPaymentRequest
    {
        public Guid OrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty; 
    }
    public sealed class RefundRequest
    {
         public string Reason { get; set; } = string.Empty;    
    }
    public sealed class CreatePaymnetIntentResponse
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public long AmountInCents { get; set; } 
        public string Currency { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
    }
    public sealed class ConfirmPaymentResponse
    {
        public bool Success { get; set; }
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public string? Message { get; set; }
    }
    public sealed class PaymnetResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public  string Status { get; set; } = string.Empty;
        public long AmountInCents { get; set; }
        public decimal AmountInDollars => AmountInCents / 100m;
        public string Currency { get; set; } = string.Empty;
        public string? CardLast4 { get; set; } 
        public string? CardBrand { get; set; } 
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    
    }
    public sealed class RefundResponse
    {
        public bool success { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}

