using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.DAL.Models.PlayerModels;

namespace TTB.DAL.Repository.Impl
{
    public class VillageRepository : Repository<VillageModel>, IVillageRepository
    {
        private const string CollectionName = "villages";
        public VillageRepository(IMongoDatabase database, ILogger<Repository<VillageModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<bool> Delete(string name, int x, int y)
        {
            var filter = Builders<VillageModel>.Filter.And(new FilterDefinition<VillageModel>[] {
                Builders<VillageModel>.Filter.Eq("villageName", name),
                Builders<VillageModel>.Filter.Eq("coordinateX", x),
                Builders<VillageModel>.Filter.Eq("coordinateY", y)
            });

            return await base.Delete(filter);
        }

        public async Task<VillageModel> GetVillage(string villageId)
        {
            var filter = Builders<VillageModel>.Filter.And(new[] {
                Builders<VillageModel>.Filter.Eq("villageId", villageId)
            });
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<VillageModel> GetVillage(int x, int y)
        {
            var filter = Builders<VillageModel>.Filter.And(new[] {
                Builders<VillageModel>.Filter.Eq("coordinateX", x),
                Builders<VillageModel>.Filter.Eq("coordinateY", y)
            });
            return (await base.Get(filter)).FirstOrDefault();
        }

        public async Task<IEnumerable<VillageModel>> GetVillages(string playerName)
        {
            var filter = Builders<VillageModel>.Filter.Eq("playerName", playerName);
            return await base.Get(filter);
        }

        public async Task<bool> Update(VillageModel model)
        {
            var filter = Builders<VillageModel>.Filter.And(new[] {
                Builders<VillageModel>.Filter.Eq("coordinateX", model.CoordinateX),
                Builders<VillageModel>.Filter.Eq("coordinateY", model.CoordinateY)
            });

            var update = Builders<VillageModel>.Update
                .Set(s => s.VillageId, model.VillageId)
                .Set(s => s.PlayerName, model.PlayerName)
                .Set(s => s.VillageName, model.VillageName)
                .Set(s => s.Alliance, model.Alliance)
                .Set(s => s.Tribe, model.Tribe)
                .Set(s => s.IsCapital, model.IsCapital)
                .Set(s => s.IsSpamFeatureOn, model.IsSpamFeatureOn)
                .Set(s => s.IsSaveResourcesFeatureOn, model.IsSaveResourcesFeatureOn)
                .Set(s => s.IsSaveTroopsFeatureOn, model.IsSaveTroopsFeatureOn)
                .Set(s => s.IsBuildFeatureOn, model.IsBuildFeatureOn)
                .Set(s => s.Types, model.Types)
                .Set(s => s.Warehourse, model.Warehourse)
                .Set(s => s.Granary, model.Granary)
                .Set(s => s.Resources, model.Resources)
                .Set(s => s.ResourcesProduction, model.ResourcesProduction)
                .Set(s => s.Attacks, model.Attacks)
                .Set(s => s.BuildingPlanId, model.BuildingPlanId)
                .Set(s => s.PreferableUnits, model.PreferableUnits)
                .Set(s => s.SpamUnits, model.SpamUnits);

            return await base.Update(filter, update);
        }

        public async Task<bool> Update(IEnumerable<VillageModel> models)
        {
            var listOfUpdateModels = new List<UpdateOneModel<VillageModel>>();
            foreach (var model in models)
            {
                var filter = Builders<VillageModel>.Filter.And(new[] {
                    Builders<VillageModel>.Filter.Eq("coordinateX", model.CoordinateX),
                    Builders<VillageModel>.Filter.Eq("coordinateY", model.CoordinateY)
                });
                var update = Builders<VillageModel>.Update
                    .Set(s => s.VillageId, model.VillageId)
                    .Set(s => s.PlayerName, model.PlayerName)
                    .Set(s => s.VillageName, model.VillageName)
                    .Set(s => s.Alliance, model.Alliance)
                    .Set(s => s.Tribe, model.Tribe)
                    .Set(s => s.IsCapital, model.IsCapital)
                    .Set(s => s.IsSpamFeatureOn, model.IsSpamFeatureOn)
                    .Set(s => s.IsSaveResourcesFeatureOn, model.IsSaveResourcesFeatureOn)
                    .Set(s => s.IsSaveTroopsFeatureOn, model.IsSaveTroopsFeatureOn)
                    .Set(s => s.IsBuildFeatureOn, model.IsBuildFeatureOn)
                    .Set(s => s.Types, model.Types)
                    .Set(s => s.Warehourse, model.Warehourse)
                    .Set(s => s.Granary, model.Granary)
                    .Set(s => s.Resources, model.Resources)
                    .Set(s => s.ResourcesProduction, model.ResourcesProduction)
                    .Set(s => s.Attacks, model.Attacks)
                    .Set(s => s.BuildingPlanId, model.BuildingPlanId)
                    .Set(s => s.PreferableUnits, model.PreferableUnits)
                    .Set(s => s.SpamUnits, model.SpamUnits);

                var updateOneModel = new UpdateOneModel<VillageModel>(filter, update)
                {
                    IsUpsert = true
                };

                listOfUpdateModels.Add(updateOneModel);
            }

            var result = await _collection.BulkWriteAsync(listOfUpdateModels);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateWatchInfo(IEnumerable<VillageModel> models)
        {
            var listOfUpdateModels = new List<UpdateOneModel<VillageModel>>();
            foreach (var model in models)
            {
                var filter = Builders<VillageModel>.Filter.And(new[] {
                    Builders<VillageModel>.Filter.Eq("coordinateX", model.CoordinateX),
                    Builders<VillageModel>.Filter.Eq("coordinateY", model.CoordinateY)
                });
                var update = Builders<VillageModel>.Update
                    .Set(s => s.Warehourse, model.Warehourse)
                    .Set(s => s.Granary, model.Granary)
                    .Set(s => s.ResourcesProduction, model.ResourcesProduction)
                    .Set(s => s.Resources, model.Resources)
                    .Set(s => s.NextBuildingPlanExecutionTime, model.NextBuildingPlanExecutionTime)
                    .Set(s => s.IsWaitingForResources, model.IsWaitingForResources)
                    .Set(s => s.Attacks, model.Attacks);
                var updateOneModel = new UpdateOneModel<VillageModel>(filter, update)
                {
                    IsUpsert = true
                };

                listOfUpdateModels.Add(updateOneModel);
            }

            var result = await _collection.BulkWriteAsync(listOfUpdateModels);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateInfos(IEnumerable<VillageModel> models)
        {
            var listOfUpdateModels = new List<UpdateOneModel<VillageModel>>();
            foreach (var model in models)
            {
                var filter = Builders<VillageModel>.Filter.And(new[] {
                    Builders<VillageModel>.Filter.Eq("coordinateX", model.CoordinateX),
                    Builders<VillageModel>.Filter.Eq("coordinateY", model.CoordinateY)
                });
                var update = Builders<VillageModel>.Update
                    .Set(s => s.VillageId, model.VillageId)
                    .Set(s => s.PlayerName, model.PlayerName)
                    .Set(s => s.VillageName, model.VillageName)
                    .Set(s => s.Alliance, model.Alliance)
                    .Set(s => s.Tribe, model.Tribe)
                    .Set(s => s.IsCapital, model.IsCapital)
                    .Set(s => s.IsSpamFeatureOn, model.IsSpamFeatureOn)
                    .Set(s => s.IsSaveResourcesFeatureOn, model.IsSaveResourcesFeatureOn)
                    .Set(s => s.IsSaveTroopsFeatureOn, model.IsSaveTroopsFeatureOn)
                    .Set(s => s.IsBuildFeatureOn, model.IsBuildFeatureOn)
                    .Set(s => s.Types, model.Types)
                    .Set(s => s.PreferableUnits, model.PreferableUnits)
                    .Set(s => s.BuildingPlanId, model.BuildingPlanId)
                    .Set(s => s.SpamUnits, model.SpamUnits);
                var updateOneModel = new UpdateOneModel<VillageModel>(filter, update)
                {
                    IsUpsert = true
                };

                listOfUpdateModels.Add(updateOneModel);
            }

            var result = await _collection.BulkWriteAsync(listOfUpdateModels);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateBaseInfos(IEnumerable<VillageModel> models)
        {
            var listOfUpdateModels = new List<UpdateOneModel<VillageModel>>();
            foreach (var model in models)
            {
                var filter = Builders<VillageModel>.Filter.And(new[] {
                    Builders<VillageModel>.Filter.Eq("coordinateX", model.CoordinateX),
                    Builders<VillageModel>.Filter.Eq("coordinateY", model.CoordinateY)
                });
                var update = Builders<VillageModel>.Update
                    .Set(s => s.VillageId, model.VillageId)
                    .Set(s => s.PlayerName, model.PlayerName)
                    .Set(s => s.VillageName, model.VillageName)
                    .Set(s => s.Alliance, model.Alliance)
                    .Set(s => s.Tribe, model.Tribe)
                    .Set(s => s.IsCapital, model.IsCapital);
                var updateOneModel = new UpdateOneModel<VillageModel>(filter, update)
                {
                    IsUpsert = true
                };

                listOfUpdateModels.Add(updateOneModel);
            }

            var result = await _collection.BulkWriteAsync(listOfUpdateModels);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteVillages(string playerName)
        {
            var filter = Builders<VillageModel>.Filter.And(new FilterDefinition<VillageModel>[] {
                Builders<VillageModel>.Filter.Eq("playerName", playerName)
            });

            return await base.Delete(filter);
        }
    }
}
