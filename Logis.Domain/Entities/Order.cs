using Logis.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.Entities
{
    public sealed class Order
    {
        // Order Related
        public Guid Id { get; private set; } = Guid.NewGuid();
        public decimal WeightKg { get; private set; } 
        public decimal DeclaredValue { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public OrderStatus Status { get; private set; } = OrderStatus.Draft;
        public Address PickupAddress { get; private set; } = null!;

        // User Related
        public Guid UserId { get; private set; }
        public string CurrencyCode { get; private set; } = "EGP";
        public Address DeliveryAddress { get; private set; } = null!;

        // Order As Entity Related
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
        public string? Notes { get; private set; }

        // Payment Related
        public Guid? PaymentId { get; private set; }
        public Payment? Payment { get; private set; }
        public bool IsPaid { get; private set; } = false;

        private readonly List<OrderItem> _items = new ();
        public IReadOnlyCollection<OrderItem> Items => _items;
    
        private Order() { }

        public Order(Guid userId, string orderNumber, decimal weightKg, decimal declaredValue, string currencyCode, string? notes, IEnumerable<OrderItem> items, Address pickupAddress, Address deliveryAddress)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId Is Requird");
            if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("order Number Is Requird");
            if (weightKg <= 0 ) throw new ArgumentOutOfRangeException(nameof(weightKg));
            if (declaredValue < 0 ) throw new ArgumentOutOfRangeException(nameof(declaredValue));
            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
                throw new ArithmeticException("Currency Code Must be ISO-4217 (3 letters).");

            UserId = userId;
            OrderNumber = orderNumber.Trim();
            PickupAddress = pickupAddress;
            DeliveryAddress = deliveryAddress;
            WeightKg = weightKg;
            DeclaredValue = declaredValue;
            CurrencyCode = currencyCode.Trim().ToUpperInvariant();
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

            foreach (var item in items ?? Array.Empty<OrderItem>())
            
                _items.Add(item);

            Touch();
        }

        private void Touch()=> UpdatedAtUtc = DateTime.UtcNow;
        public void Confirm()
        {
            if (!IsPaid)
                throw new InvalidOperationException("Order Must Be Paid To Be Confirmed.");

            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException("Only Draft Orders Can Be Confirmed.");

            Status = OrderStatus.Confrimed;
            Touch();
        }
        public void MarkAsPaid(Guid paymentId)
        {
            if (IsPaid)
                return;

            if (Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot Mark Cancelled Order As Paid.");

            IsPaid = true;
            PaymentId = paymentId;
            Touch();
        }
        public void Cancel(string? reason)
        {
            if (Status == OrderStatus.Delivered) throw new InvalidOperationException("Delivered Orders Can Not Be Cancelled");
            if (Status == OrderStatus.InTransit) throw new InvalidOperationException("InTransit Orders Can Not Be Cancelled");
            if (Status == OrderStatus.Cancelled) return;

            Notes = string.IsNullOrWhiteSpace(reason) ? Notes : reason.Trim();
            Status = OrderStatus.Cancelled;
            Touch();
        }

    }
}
