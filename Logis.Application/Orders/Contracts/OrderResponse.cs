using Logis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Orders.Contracts
{
    public sealed class OrderResponse
    {
        public required Guid Id { get; init; }
        public required string OrderNumber { get; init; }
        public required OrderStatus Status { get; init; }
        public required Guid UserId { get; init; }
        public required AddressDto Pickup { get; init; }
        public required AddressDto Delivery { get; init; }
        public required string? Notes { get; init; }
        public required decimal WeightKg { get; init; }
        public required decimal DeclaredValue { get; init; }
        public required string CurrencyCode { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required DateTime UpdatedAtUtc { get; init; }
        public required List<OrderItemResponse> Items { get; init; }
        public required decimal ItemsTotal { get; init; }
    }
    public sealed class OrderItemResponse
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Sku { get; init; }
        public required int Quantity { get; init; }
        public required decimal UnitPrice { get; init; }
        public required decimal LineTotal { get; init; }

    }
}
