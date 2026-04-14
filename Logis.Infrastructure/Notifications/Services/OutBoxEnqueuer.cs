using Logis.Application.Notifications.Services;
using Logis.Infrastructure.Persistence;
using Logis.Infrastructure.Persistence.OutBox;
using System.Text.Json;

namespace Logis.Infrastructure.Notifications.Services
{
    public sealed class OutBoxEnqueuer : IOutBoxEnqueuer
    {
        private readonly AppDbContext db;

        public OutBoxEnqueuer(AppDbContext db)
        {
            this.db = db;
        }

        public Task EnqueueAsync(string type, object payload)
        {
            // payload Is Not In The Right Format To Store In Db
            var serializedPayload = JsonSerializer.Serialize(payload);
            db.OutBoxMessages.Add(new OutBoxMessage(type, serializedPayload));
            return Task.CompletedTask;
        }
    }
}
