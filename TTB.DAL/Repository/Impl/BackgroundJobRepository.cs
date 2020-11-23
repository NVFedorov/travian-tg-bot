using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.DAL.Models;

namespace TTB.DAL.Repository.Impl
{
    public class BackgroundJobRepository : Repository<BackgroundJobModel>, IBackgroundJobRepository
    {
        private const string CollectionName = "backgroundJobs";
        public BackgroundJobRepository(IMongoDatabase database, ILogger<Repository<BackgroundJobModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<bool> Delete(BackgroundJobModel item)
        {
            var filter = Builders<BackgroundJobModel>.Filter.Eq("BackgroundJobId", item.BackgroundJobId);
            return await base.Delete(filter);
        }

        public async Task<BackgroundJobModel> GetBackgroundJobForUser(string botUserName)
        {
            var filter = Builders<BackgroundJobModel>.Filter.Eq("BotUserName", botUserName);
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<bool> Update(BackgroundJobModel item)
        {
            var filter = Builders<BackgroundJobModel>.Filter.Eq("BackgroundJobId", item.BackgroundJobId);
            var update = Builders<BackgroundJobModel>.Update
                .Set(s => s.BotUserName, item.BotUserName)
                .Set(s => s.IsRunning, item.IsRunning)
                .Set(s => s.CommandsQueue, item.CommandsQueue);

            return await base.Update(filter, update);
        }

        public override async Task Insert(BackgroundJobModel item)
        {
            await base.Insert(item);
        }

        Task<BackgroundJobModel> IBackgroundJobRepository.GetBackgroundJobForUser(string botUserName)
        {
            throw new System.NotImplementedException();
        }
    }
}
