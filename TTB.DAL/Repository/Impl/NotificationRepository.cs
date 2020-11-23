using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.Common.Extensions;
using TTB.DAL.Models.ScenarioModels;
using TTB.DAL.Models.ScenarioModels.Enums;

namespace TTB.DAL.Repository.Impl
{
    public class NotificationRepository : Repository<IncomingAttackNotificationModel>, INotificationRepository
    {
        private const string CollectionName = "incomingAttacks";

        public NotificationRepository(IMongoDatabase database, ILogger<Repository<IncomingAttackNotificationModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<List<IncomingAttackNotificationModel>> GetIncomingAttacksAndScans(string userName)
        {
            var filter = Builders<IncomingAttackNotificationModel>.Filter.And(new[] {
                Builders<IncomingAttackNotificationModel>.Filter.Or(new[]
                {
                    Builders<IncomingAttackNotificationModel>.Filter.Eq("status", WatchStatus.ATTACK_ALERT.GetEnumDisplayName()),
                    Builders<IncomingAttackNotificationModel>.Filter.Eq("status", WatchStatus.SCAN_ALERT.GetEnumDisplayName())
                }),
                Builders<IncomingAttackNotificationModel>.Filter.Eq("userName", userName)
            });

            return await base.Get(filter);
        }
    }
}
