using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;

namespace TTB.DAL.Repository.Impl
{
    public class BuildingRepository : KnowledgeRepository<BuildingModel>, IBuildingRepository
    {
        private const string _collectionName = "buildings";
        private readonly ILogger<BuildingRepository> _logger;

        public BuildingRepository(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, _collectionName, loggerFactory.CreateLogger<Repository<BuildingModel>>())
        {
            _logger = loggerFactory.CreateLogger<BuildingRepository>();
        }

        public async Task<bool> Delete(string id)
        {
            var filter = Builders<BuildingModel>.Filter.And(new FilterDefinition<BuildingModel>[] {
                Builders<BuildingModel>.Filter.Eq("_id", ObjectId.Parse(id))
            });

            return await base.Delete(filter);
        }

        public async Task<IEnumerable<BuildingModel>> GetAllBuildings()
        {
            IEnumerable<BuildingModel> result = null;
            try
            {
                result = await _collection.AsQueryable().ToListAsync();
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemException, exc, exc.Message);
            }

            return result;
        }

        public async Task<BuildingModel> GetBuilding(string id)
        {
            var filter = Builders<BuildingModel>.Filter.And(new FilterDefinition<BuildingModel>[] {
                Builders<BuildingModel>.Filter.Eq("_id", ObjectId.Parse(id))
            });

            return (await base.Get(filter))?.FirstOrDefault();
        }

        public async Task<bool> Update(BuildingModel item)
        {
            var filter = Builders<BuildingModel>.Filter.And(new FilterDefinition<BuildingModel>[] {
                Builders<BuildingModel>.Filter.Eq("_id", ObjectId.Parse(item._id))
            });

            var result = false;
            try
            {
                var replaceResult = await _collection.ReplaceOneAsync(filter, item);
                result = replaceResult.IsAcknowledged && replaceResult.ModifiedCount == 1;
            }
            catch(Exception exc)
            {
                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message);
            }

            return result;
        }
    }
}
