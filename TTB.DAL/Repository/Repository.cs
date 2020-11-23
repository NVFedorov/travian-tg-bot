using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.Common.Settings;

namespace TTB.DAL.Repository
{
    public abstract class Repository<T>
    {
        protected readonly IMongoCollection<T> _collection;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(IMongoDatabase database, string collectionName, ILogger<Repository<T>> logger)
        {
            this._collection = database.GetCollection<T>(collectionName);
            this._logger = logger;
        }

        public virtual async Task<List<T>> GetAll()
        {
            try
            {
                return await this._collection.Find(x => true).ToListAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(LoggingEvents.GetItemNotFound, ex, ex.Message);
                throw ex;
            }
        }

        public virtual async Task<List<T>> Get(FilterDefinition<T> filter)
        {
            try
            {
                return await this._collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(LoggingEvents.GetItemNotFound, ex, ex.Message);
                throw ex;
            }
        }

        public virtual async Task Insert(T item)
        {
            try
            {
                await this._collection.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                this._logger.LogError(LoggingEvents.InsertItemException, ex, ex.Message);
                throw ex;
            }
        }

        public virtual async Task<bool> Update(FilterDefinition<T> filter, UpdateDefinition<T> item)
        {
            try
            {
                var actionResult = await this._collection.UpdateOneAsync(filter, item);
                return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                this._logger.LogError(LoggingEvents.UpdateItemException, ex, ex.Message);
                throw ex;
            }
        }

        public virtual async Task<bool> Delete(FilterDefinition<T> filter)
        {
            try
            {
                var actionResult = await this._collection.DeleteManyAsync(filter);
                return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                this._logger.LogError(LoggingEvents.DeleteItemException, ex, ex.Message);
                throw ex;
            }
        }
    }
}
