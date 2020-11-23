using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TTB.DAL.Models;

namespace TTB.DAL.Repository.Impl
{
    public class ManagerLogRepository : Repository<ManagerLogEntryModel>, IManagerLogRepository
    {
        private const string CollectionName = "managerLogs";
        public ManagerLogRepository(IMongoDatabase database, ILogger<Repository<ManagerLogEntryModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<List<ManagerLogEntryModel>> GetLast(int numberOfRecords, string filter)
        {
            var collection = base._collection
                .AsQueryable()
                .OrderByDescending(x => x.UtcTimestamp)
                .Take(numberOfRecords);

            if (!string.IsNullOrEmpty(filter))
            {
                collection = collection.Where(x => x.Level == filter);
            }

            return await collection.ToListAsync();
        }

        public async Task<List<ManagerLogEntryModel>> GetLast(DateTime start, string filter)
        {
            var collection = base._collection
                .AsQueryable()
                .OrderByDescending(x => x.UtcTimestamp)
                .Where(x => x.UtcTimestamp > start);

            if (!string.IsNullOrEmpty(filter))
            {
                collection = collection.Where(x => x.Level == filter);

            }

            return await collection.ToListAsync();
        }
    }
}
