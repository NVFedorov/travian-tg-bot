using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.Common.Extensions;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository.Impl
{
    public class ArmyEventRepository : Repository<ArmyEventModel>, IArmyEventRepository
    {
        private const string CollectionName = "armyEvents";
        public ArmyEventRepository(IMongoDatabase database, ILogger<Repository<ArmyEventModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<List<ArmyEventModel>> GetArmyEvents(string userName, DateTimeOffset dateTime)
        {
            var filter = Builders<ArmyEventModel>.Filter.And(new[] {
                Builders<ArmyEventModel>.Filter.Gt("dateTime", dateTime.ToFormattedString()),
                Builders<ArmyEventModel>.Filter.Eq("userName", userName)
            });

            return await base.Get(filter);
        }

        public async Task<List<ArmyEventModel>> GetNewEvents(string userName)
        {
            var filter = Builders<ArmyEventModel>.Filter.And(new[] {
                Builders<ArmyEventModel>.Filter.Gt("dateTime", DateTimeOffset.UtcNow.ToFormattedString()),
                Builders<ArmyEventModel>.Filter.Eq("isNew", true),
                Builders<ArmyEventModel>.Filter.Eq("userName", userName)
            });

            return await base.Get(filter);
        }

        public async Task<bool> RemoveOldArmyEvents(string userName)
        {
            var filter = Builders<ArmyEventModel>.Filter.And(new[] {
                Builders<ArmyEventModel>.Filter.Lt("dateTime", DateTimeOffset.UtcNow.AddDays(-1).ToFormattedString()),
                Builders<ArmyEventModel>.Filter.Eq("userName", userName)
            });

            return await base.Delete(filter);
        }

        public async Task<bool> Update(ArmyEventModel item)
        {
            var filter = Builders<ArmyEventModel>.Filter.Eq("eventId", item.EventId);
            var update = Builders<ArmyEventModel>.Update
                .Set(s => s.IsNew, item.IsNew)
                .Set(s => s.ArePreparationsDone, item.ArePreparationsDone);

            return await base.Update(filter, update);
        }

        public async Task<bool> Update(List<ArmyEventModel> items)
        {
            var tasks = new List<Task<bool>>();
            foreach (var item in items)
            {
                tasks.Add(this.Update(item));
            }

            var updateRes = await Task.WhenAll(tasks);
            return updateRes.All(x => x);
        }
    }
}
