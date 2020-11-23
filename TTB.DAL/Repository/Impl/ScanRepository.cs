using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository.Impl
{
    public class ScanRepository : Repository<ScanNotificationModel>, IScanRepository
    {
        private const string CollectionName = "scanEvents";
        public ScanRepository(IMongoDatabase database, ILogger<Repository<ScanNotificationModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<List<ScanNotificationModel>> GetScans(string userName)
        {
            var filter = Builders<ScanNotificationModel>.Filter.Eq("userName", userName);
            return await base.Get(filter);
        }
        
        public async Task<bool> Update(ScanNotificationModel scan)
        {
            var filter = Builders<ScanNotificationModel>.Filter.Eq("scanId", scan.ScanId);
            var update = Builders<ScanNotificationModel>.Update
                .Set(s => s.WasShown, scan.WasShown);

            return await base.Update(filter, update);
        }

        public async Task<bool> Update(IEnumerable<ScanNotificationModel> scans)
        {
            var tasks = new List<Task<bool>>();
            foreach (var scan in scans)
            {
                tasks.Add(this.Update(scan));
            }

            var updateRes = await Task.WhenAll(tasks);
            return updateRes.All(x => x);
        }
    }
}
