using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using TTB.DAL.Models;

namespace TTB.DAL.Repository.Impl
{
    public class TravianUserRepository : Repository<TravianUser>, ITravianUserRepository
    {
        private const string CollectionName = "users";

        public TravianUserRepository(IMongoDatabase database, ILogger<TravianUserRepository> logger) : base(database, CollectionName, logger)
        {
            var indexModel = new CreateIndexModel<TravianUser>(Builders<TravianUser>.IndexKeys.Ascending(x => x.UserName));
            _collection.Indexes.CreateOne(indexModel);
        }

        public async Task<bool> Delete(string userName, string botUserName)
        {
            var filter = Builders<TravianUser>.Filter.And(new FilterDefinition<TravianUser>[] {
                Builders<TravianUser>.Filter.Eq("userName", userName),
                Builders<TravianUser>.Filter.Eq("botUserName", botUserName)
            });

            return await base.Delete(filter);
        }

        public async Task<TravianUser> GetActiveUser(string botUserName)
        {
            var filter = Builders<TravianUser>.Filter.And(new[] {
                Builders<TravianUser>.Filter.Eq("botUserName", botUserName),
                Builders<TravianUser>.Filter.Eq("isActive", true)
            });
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<TravianUser> GetUserByName(string userName, string botUserName)
        {
            var filter = Builders<TravianUser>.Filter.And(new FilterDefinition<TravianUser>[] {
                Builders<TravianUser>.Filter.Eq("userName", userName),
                Builders<TravianUser>.Filter.Eq("botUserName", botUserName)
            });
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<IEnumerable<TravianUser>> GetUsersByBotUser(string botUserName)
        {
            var filter = Builders<TravianUser>.Filter.Eq("botUserName", botUserName);
            return await base.Get(filter);
        }

        public async Task<bool> Update(TravianUser user)
        {
            var filter = Builders<TravianUser>.Filter.Eq("_id", ObjectId.Parse(user.InternalId));
            var update = Builders<TravianUser>.Update
                .Set(s => s.Password, user.Password)
                .Set(s => s.IsActive, user.IsActive)
                //.Set(s => s.TimeZoneId, user.TimeZoneId)
                .Set(s => s.Url, user.Url)
                .Set(s => s.Cookies, user.Cookies)
                .Set(s => s.ExecutionContext, user.ExecutionContext)
                .Set(s => s.PlayerData, user.PlayerData);
                //.Set(s => s.PlayerData.Alliance, user.PlayerData?.Alliance)
                //.Set(s => s.PlayerData.Status, user.PlayerData?.Status)
                //.Set(s => s.PlayerData.TimeZone, user.PlayerData?.TimeZone)
                //.Set(s => s.PlayerData.Tribe, user.PlayerData?.Tribe)
                //.Set(s => s.PlayerData.VillagesIds, user.PlayerData?.VillagesIds);
            
            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.ModifiedCount > 0;
        }

        public async Task<bool> Update(IEnumerable<TravianUser> users)
        {
            var tasks = new List<Task<bool>>();
            foreach (var user in users)
            {
                tasks.Add(this.Update(user));
            }

            var updateRes = await Task.WhenAll(tasks);
            return updateRes.All(x => x);
        }

        public async Task<bool> ReplacePlayerData(TravianUser user)
        {
            var filter = Builders<TravianUser>.Filter.Eq("_id", ObjectId.Parse(user.InternalId));
            var update = Builders<TravianUser>.Update
                .Set(s => s.PlayerData, user.PlayerData);

            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ReplacePlayerDataVillages(TravianUser user)
        {
            var filter = Builders<TravianUser>.Filter.Eq("_id", ObjectId.Parse(user.InternalId));
            var update = Builders<TravianUser>.Update
                .Set(s => s.Cookies, user.Cookies)
                .Set(s => s.PlayerData.VillagesIds, user.PlayerData?.VillagesIds);

            var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            return result.ModifiedCount > 0;
        }
    }
}
