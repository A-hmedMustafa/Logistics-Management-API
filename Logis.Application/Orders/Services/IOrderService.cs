using Logis.Application.Orders.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Application.Orders.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateAsync(Guid currentUserId, CreateOrderRequest request);
        Task<bool> ConfirmAsync(Guid currentUserId, Guid orderId);
        Task<OrderResponse?> GetAsync(Guid currentUserId, Guid orderId);
        Task<IReadOnlyList<OrderResponse>> ListAsync(Guid currentUserId,int page, int pageSize);
        Task<bool> CancelAsync(Guid currentUserId,Guid orderId,string? reason);
    }
}
