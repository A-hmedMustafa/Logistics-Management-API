using Logis.Application.Notifications.OutboxEvents;
using Logis.Domain.Entities;
using Logis.Infrastructure.Notifications.Policy;
using Logis.Infrastructure.Persistence;
using Logis.Infrastructure.Persistence.OutBox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Logis.Infrastructure.Notifications.Services
{
    public sealed class OutboxProcessorHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorHostedService> _logger;
        private readonly string _workerId = $"worker-{Guid.NewGuid():N}";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private const int MaxAttempts = OutboxPolicy.MaxAttempts; // We reference OutboxPolicy to avoid drift between worker + admin queries.
        public OutboxProcessorHostedService(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Worker Started. workerId={WorkerId}", _workerId);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.UtcNow;
                    var lockUntil = now.AddSeconds(30);

                    var candidateIds = await db.OutBoxMessages.
                        Where(x => x.ProcessedAtUtc == null && x.Attempts < 10 && x.IgnoredAtUtc == null && (x.LockedUntilUtc == null || x.LockedUntilUtc < now))
                        .OrderBy(x => x.OccurredAtUtc)
                        .Select(x => x.Id)
                        .Take(50)
                        .ToListAsync(stoppingToken);

                    if(candidateIds.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2),stoppingToken);
                        continue;
                    }
                    // 2) ✅ Claim atomically (multi-instance safe)
                    var claimedMessages = await db.OutBoxMessages
                        .Where(x=> candidateIds.Contains(x.Id) && x.ProcessedAtUtc == null && x.IgnoredAtUtc == null &&  (x.LockedUntilUtc == null || x.LockedUntilUtc < now))
                        .ExecuteUpdateAsync(setters => setters.SetProperty(x=>x.LockedBy, _workerId)
                        .SetProperty(x=>x.LockedUntilUtc, lockUntil),stoppingToken);


                    if (claimedMessages == 0)
                    {
                        // Another instance won the race
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        continue;
                    }
                    var unsentMessages = await db.OutBoxMessages
                        .Where(ob => ob.ProcessedAtUtc == null && ob.IgnoredAtUtc == null && ob.LockedBy == _workerId && ob.LockedUntilUtc == lockUntil)
                        .OrderBy(ob => ob.OccurredAtUtc)
                        .ToListAsync(stoppingToken);

                    foreach ( var message in unsentMessages)
                    {
                        try
                        {
                            await ProcessMessageAsync(db, message,stoppingToken);
                            message.ProcessedAtUtc = DateTime.UtcNow;
                            message.LastError = null;
                            message.ClearLock();                           
                        }
                        catch( Exception ex ) 
                        {
                            message.Attempts += 1;
                            message.LastError = ex.Message;
                            message.ClearLock();
                            if(message.Attempts >= MaxAttempts && message.DeadAtUtc is null)
                                message.DeadAtUtc = DateTime.UtcNow;

                            _logger.LogWarning("Outbox Message Failed. Id={Id} Type={Type} Attempts={Attempts} workerId={WorkerId}",
                                message.Id,message.Type,message.Attempts,_workerId);
                        }
                    }

                    
                        await db.SaveChangesAsync(stoppingToken);
                    
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Outbox worker loop error. WorkerId={WorkerId}", _workerId);
                }
                await Task.Delay(TimeSpan.FromSeconds(2),stoppingToken);
            }
        }
        private static async Task ProcessMessageAsync(AppDbContext db, OutBoxMessage outBoxMessage, CancellationToken cancellationToken)
        {
            switch(outBoxMessage.Type)
            {
                case OutboxMessageTypes.OrderCreated:
                    {
                        var payload = JsonSerializer.Deserialize<OrderCreatedOutBoxEvent>(outBoxMessage.Payload, JsonOptions) ??
                            throw new InvalidOperationException("Invalid OrderCreated Payload");

                        
                        if (await AlreadyCreatedAsync(db,outBoxMessage.Id,cancellationToken)) return;

                        db.Notifications.Add(new Notification(
                            payload.UserId, 
                            outBoxMessage.Id,
                            "Order Created",
                            $"Your Order {payload.OrderNumber} Was Created.", 
                            $"/Orders/{payload.OrderId}"));

                        return;
                    }
                case OutboxMessageTypes.ShipmentAssigned:
                    {
                        var payload = JsonSerializer.Deserialize<ShipmentAssignedOutboxEvent>(outBoxMessage.Payload, JsonOptions) ??
                            throw new InvalidOperationException("Invalid Shipment Assigned Payload.");

                        if (await AlreadyCreatedAsync(db, outBoxMessage.Id, cancellationToken)) return;

                        db.Notifications.Add(new Notification(
                            payload.UserId,
                            outBoxMessage.Id,
                            "Shipment Assigned",
                            $"Carrier '{payload.CarrierName}' was assigned to your shipment ({payload.TrackingCode}).",
                            $"/Tracking/{payload.TrackingCode}"
                        ));
                        return;
                    }
                case OutboxMessageTypes.ShipmentStatusChanged:
                    {
                        var payload = JsonSerializer.Deserialize<ShipmentStatusChangedOutboxEvent>(outBoxMessage.Payload, JsonOptions) ??
                            throw new InvalidOperationException("Invalid Shipment Status Changed Payload.");

                        if (await AlreadyCreatedAsync(db, outBoxMessage.Id, cancellationToken)) return;

                        db.Notifications.Add(new Notification(
                            payload.UserId,
                            outBoxMessage.Id,
                            "Shipment Update",
                            $"Shipment {payload.TrackingCode} Is Now {payload.NewStatus}",
                            $"/Tracking/{payload.TrackingCode}"
                        ));

                        return;
                    }
                default:
                    throw new InvalidOperationException($"Unknown Outbox Message Type {outBoxMessage.Type}");
            }
        
        }
        private static Task<bool> AlreadyCreatedAsync(AppDbContext db, Guid outboxMessageId, CancellationToken cancellationToken)
        {
            return db.Notifications.AnyAsync(n=>n.SourceOutboxMessageId == outboxMessageId,cancellationToken);
        }
    }

}