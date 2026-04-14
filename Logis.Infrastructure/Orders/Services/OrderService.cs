using Logis.Api.Observability;
using Logis.Application.Authz.Contracts;
using Logis.Application.Authz.Services;
using Logis.Application.Notifications.OutboxEvents;
using Logis.Application.Notifications.Services;
using Logis.Application.Orders.Contracts;
using Logis.Application.Orders.Services;
using Logis.Domain.Entities;
using Logis.Domain.ValueObjects;
using Logis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Infrastructure.Orders.Services
{
    public sealed class OrderService:IOrderService
    {
        private readonly AppDbContext db;
        private readonly IOwnershipGaurdService ownership;
        private readonly IDomainEventLogger domainEventLogger;
        private readonly IOutBoxEnqueuer boxEnqueuer;
        public OrderService(AppDbContext db, IOwnershipGaurdService ownership, IDomainEventLogger domainEventLogger, IOutBoxEnqueuer boxEnqueuer)
        {
            this.db = db;
            this.ownership = ownership;
            this.domainEventLogger = domainEventLogger;
            this.boxEnqueuer = boxEnqueuer;
        }

        public async Task<OrderResponse> CreateAsync(Guid currentUserId, CreateOrderRequest request)
        {
            // 1) Validate Common Issues That Might Be In The Order Creation Request
            if (request.Items is null || request.Items.Count == 0)
                throw new ArgumentException("Order Must Contain Atleast 1 Item.");
            if (request.WeightKg <= 0)
                throw new ArgumentException("Order Weight Must be > 0.");
            if (request.CurrencyCode is null || request.CurrencyCode.Trim().Length != 3)
                throw new ArgumentException("Currency Code Must Be 3 Letters (ISO-4217)");

            // 2) Extract Pickup & Delivery Addresses From The Request
            var pickup = new Address(
                request.PickupAddress.ContactName,
                request.PickupAddress.Phone,
                request.PickupAddress.Line1,
                request.PickupAddress.Line2,
                request.PickupAddress.City,
                request.PickupAddress.State,
                request.PickupAddress.PostalCode,
                request.PickupAddress.CountryCode);
            var delivery = new Address(
                request.DeliveryAddress.ContactName,
                request.DeliveryAddress.Phone,
                request.DeliveryAddress.Line1,
                request.DeliveryAddress.Line2,
                request.DeliveryAddress.City,
                request.DeliveryAddress.State,
                request.DeliveryAddress.PostalCode,
                request.DeliveryAddress.CountryCode);

            // 3) Make A unique User-Friendly Order Number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{GenerateCode(6)}";

            // 4) Extract The Order Items From The Reuest 
            var orderItems = request.Items.Select(i => new OrderItem(i.Name, i.Sku, i.Quantity, i.UnitPrice)).ToList();

            // 5) Construct The Actuall Order 
            var order = new Order(
                userId:currentUserId,
                orderNumber: orderNumber,
                weightKg:request.WeightKg,
                declaredValue:request.DeclaredValue,
                currencyCode:request.CurrencyCode,
                notes:request.Note,
                items:orderItems,
                pickupAddress:pickup,
                deliveryAddress:delivery);

            // 6) Add The Order To The DB And Apply Changes
            db.Orders.Add(order);

            // 7) Add An OutBoxMessage That Say We Need To Send A Notification For The Customer Of This Order Creation

            await boxEnqueuer.EnqueueAsync("Order Created", new OrderCreatedOutBoxEvent
                (
                    UserId: currentUserId,
                    OrderId: order.Id,
                    OrderNumber: order.OrderNumber
                ));
            await db.SaveChangesAsync();
            // 8) 
            await db.Entry(order).Collection(o => o.Items).LoadAsync();

            // 9) Log The Event 
            domainEventLogger.OrderCreated(order.Id, currentUserId);
            // 10) Map The Order To Order View Model (OrderResponse)
            return Map(order);
        }
        public async Task<bool> ConfirmAsync(Guid currentUserId, Guid orderId)
        {
            var orderToConfirm = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (orderToConfirm is null) 
                return false;

            if(ownership.MustOwn(currentUserId,orderToConfirm.UserId) != AuthorizationResult.Allowed)
                return false;

            orderToConfirm.Confirm();
            await db.SaveChangesAsync();
            domainEventLogger.OrderConfirmed(orderId, currentUserId);
            return true;
        }
        public async Task<OrderResponse?> GetAsync(Guid currentUserId, Guid orderId)
        {
            // 1) Get The Order With The Same Id From The Db
            var order = await db.Orders.Include(o=>o.Items).FirstOrDefaultAsync(o => o.Id == orderId);

            // 2) Validate Order
            if (order is null)
                return null;

            // 3) Make Sure The User Who Made The Order Is The One Who Is Trying To Retrieve It
            var authResult = ownership.MustOwn(currentUserId, order.UserId);
            if (authResult != AuthorizationResult.Allowed)
                return null;

            // 4) Map The Order To Order View Model (OrderResponse)
            return Map(order);
        }
        public async Task<IReadOnlyList<OrderResponse>> ListAsync(Guid currentUserId, int page, int pageSize)
        {
            // 1) Set The Rules Of Pagenation
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            // 2) Get The All The Orders For This Customer(User) 
            var orders = await db.Orders.AsNoTracking()
                .Where(o => o.UserId == currentUserId)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Items)
                .ToListAsync();

            // 3) Map The Orders To View Models
            return orders.Select(Map).ToList();
        }

        public async Task<bool> CancelAsync(Guid currentUserId, Guid orderId, string? reason)
        {
            var order = await db.Orders.Include(o=>o.Items).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                return false;
            var authResult = ownership.MustOwn(currentUserId, order.UserId);
            if (authResult != AuthorizationResult.Allowed)
                return false;
            order.Cancel(reason);

            await db.SaveChangesAsync();
            domainEventLogger.OrderCancelled(orderId,currentUserId,reason);
            return true;
        }

        // Generates A Random Number To Be Added To Order Number
        private static string GenerateCode(int len)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = Random.Shared;
            return new string(Enumerable.Range(0, len).Select(_ => alphabet[rng.Next(alphabet.Length)]).ToArray());
        }
        // Mapping The Order Info Into The ViewModel That Will Be Sent To The UI
        private static OrderResponse Map(Order order)
        {
            // 1) We Extract The Order Items To A List 
            var orderItems = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                Name = i.Name,
                Sku = i.Sku,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal()
            }).ToList();

            // 2) Extracting The Order Info Into The OrderResponse, beacuse That What Is Going To Be Sent To The UI 
            return new OrderResponse 
            {
                Id= order.Id,
                Status = order.Status,
                CurrencyCode = order.CurrencyCode,
                UserId = order.UserId,
                OrderNumber = order.OrderNumber,
                Pickup = new AddressDto 
                {
                    ContactName = order.PickupAddress.ContactName,
                    Phone = order.PickupAddress.Phone,
                    Line1 = order.PickupAddress.Line1,
                    Line2 = order.PickupAddress.Line2,
                    City = order.PickupAddress.City,
                    State = order.PickupAddress.State,
                    PostalCode = order.PickupAddress.PostalCode,
                    CountryCode = order.PickupAddress.CountryCode
                },
                Delivery = new AddressDto 
                {
                    ContactName = order.DeliveryAddress.ContactName,
                    Phone = order.DeliveryAddress.Phone,
                    Line1 = order.DeliveryAddress.Line1,
                    Line2 = order.DeliveryAddress.Line2,
                    City = order.DeliveryAddress.City,
                    State = order.DeliveryAddress.State,
                    PostalCode = order.DeliveryAddress.PostalCode,
                    CountryCode = order.DeliveryAddress.CountryCode
                },
                WeightKg = order.WeightKg,
                DeclaredValue = order.DeclaredValue,
                Notes = order.Notes,
                Items = orderItems,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
                ItemsTotal = orderItems.Sum(oi=>oi.LineTotal)
                

            };
        }

    }
}
