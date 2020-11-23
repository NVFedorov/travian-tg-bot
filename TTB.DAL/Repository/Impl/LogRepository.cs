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
    public class LogRepository<T> : Repository<T>, ILogRepository<T> where T : LogEntryModel
    {
        private const string ManagerCollectionName = "managerLogs";
        private const string WebApiCollectionName = "logs";
        public LogRepository(IMongoDatabase database, ILogger<Repository<T>> logger) 
            : base(database, 
                  typeof(T) == typeof(ManagerLogEntryModel) 
                      ? ManagerCollectionName 
                      : WebApiCollectionName, 
                  logger)
        {
        }

        public async Task<List<T>> GetLast(int numberOfRecords, string filter)
        {
            var collection = base._collection
                .AsQueryable()
                .OrderByDescending(x => x.Timestamp)
                .Take(numberOfRecords);

            if (!string.IsNullOrEmpty(filter))
            {
                collection = collection.Where(x => x.Level == filter);
            }

            return await collection.ToListAsync();
        }

        public async Task<List<T>> GetLast(DateTime start, string filter)
        {
            var collection = base._collection
                .AsQueryable()
                .OrderByDescending(x => x.Timestamp)
                .Where(x => x.Timestamp > start);

            if (!string.IsNullOrEmpty(filter))
            {
                collection = collection.Where(x => x.Level == filter);

            }

            return await collection.ToListAsync();
        }
    }
}
