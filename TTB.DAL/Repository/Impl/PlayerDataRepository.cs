using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository.Impl
{
    public class PlayerDataRepository : Repository<PlayerDataModel>, IPlayerDataRepository
    {
        private const string CollectionName = "playerData";

        public PlayerDataRepository(IMongoDatabase database, ILogger<Repository<PlayerDataModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<bool> Delete(string travianUserName)
        {
            var filter = Builders<PlayerDataModel>.Filter.And(new FilterDefinition<PlayerDataModel>[] {
                Builders<PlayerDataModel>.Filter.Eq("userName", travianUserName),
            });

            return await base.Delete(filter);
        }

        public async Task<PlayerDataModel> GetPlayerDataForUser(string travianUserName)
        {
            var filter = Builders<PlayerDataModel>.Filter.Eq("userName", travianUserName);
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<bool> Update(PlayerDataModel item)
        {
            //var updates = new List<UpdateOneModel<PlayerDataModel>>();
            //foreach (var village in item.Villages)
            //{
            //    var filter = Builders<PlayerDataModel>.Filter.Where(x => x.UserName == item.UserName && x.Villages.Any(y => y.VillageId == village.VillageId));
            //    var update = new UpdateOneModel<PlayerDataModel>(
            //        filter,
            //        Builders<PlayerDataModel>.Update
            //            .Set(s => s.Villages.ElementAt(-1).IsSaveResourcesFeatureOn, village.IsSaveResourcesFeatureOn)
            //            .Set(s => s.Villages.ElementAt(-1).IsSaveTroopsFeatureOn, village.IsSaveResourcesFeatureOn)
            //            .Set(s => s.Villages.ElementAt(-1).Status, village.Status)
            //            .Set(s => s.Villages.ElementAt(-1).Types, village.Types));

            //    updates.Add(update);
            //}

            //var result = await this.collection.BulkWriteAsync(updates);

            var filter = Builders<PlayerDataModel>.Filter.Eq("userName", item.UserName);
            var update = Builders<PlayerDataModel>.Update
                .Set(s => s.Status, item.Status)
                .Set(s => s.Tribe, item.Tribe)
                .Set(s => s.TimeZone, item.TimeZone)
                .Set(s => s.Alliance, item.Alliance)
                .Set(s => s.VillagesIds, item.VillagesIds);

            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.ModifiedCount > 0;
        }
    }
}
