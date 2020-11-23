using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TTB.Common.Settings;
using TTB.DAL.Models;

namespace TTB.DAL.Repository.Impl
{
    public class BuildingPlanRepository : Repository<BuildingPlanModel>, IBuildingPlanRepository
    {
        public static string DefaultPlanUserName = "DefaultUserName_210";

        private const string CollectionName = "buildingPlans";
        private readonly ILogger<BuildingPlanRepository> _logger;

        public BuildingPlanRepository(IMongoDatabase database, ILoggerFactory loggerFactory) 
            : base(database, CollectionName, loggerFactory.CreateLogger<Repository<BuildingPlanModel>>())
        {
            _logger = loggerFactory.CreateLogger<BuildingPlanRepository>();
        }

        public async Task<bool> Delete(string id)
        {
            var filter = Builders<BuildingPlanModel>.Filter.And(new FilterDefinition<BuildingPlanModel>[] {
                Builders<BuildingPlanModel>.Filter.Eq("_id", ObjectId.Parse(id))
            });

            return await base.Delete(filter);
        }

        public async Task<BuildingPlanModel> GetBuildingPlan(string id)
        {
            var filter = Builders<BuildingPlanModel>.Filter.And(new[] {
                Builders<BuildingPlanModel>.Filter.Eq("_id", ObjectId.Parse(id))
            });

            return (await base.Get(filter))?.FirstOrDefault();
        }

        public async Task<IEnumerable<BuildingPlanModel>> GetBuildingPlans(string botUserName)
        {
            IEnumerable<BuildingPlanModel> result = null;
            try
            {
                result = await _collection.AsQueryable().Where(x => x.BotUserName == botUserName).ToListAsync();
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemException, exc, exc.Message);
            }
            
            return result;
        }

        public async Task<bool> Update(BuildingPlanModel item)
        {
            var filter = Builders<BuildingPlanModel>.Filter.And(new[] {
                Builders<BuildingPlanModel>.Filter.Eq("_id", ObjectId.Parse(item._id))
            });

            var update = Builders<BuildingPlanModel>.Update
                .Set(s => s.Name, item.Name)
                .Set(s => s.VillageType, item.VillageType)
                .Set(s => s.BuildingSteps, item.BuildingSteps);

            return await base.Update(filter, update);
        }
    }
}
