using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TTB.Common.Extensions;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;

namespace TTB.DAL.Repository.Impl
{
    public class UnitRepository : KnowledgeRepository<UnitModel>, IUnitRepository
    {
        private const string CollectionName = "units";

        public UnitRepository(IMongoDatabase database, ILogger<Repository<UnitModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<IEnumerable<UnitModel>> GetAllUnits()
        {
            return await base.GetAll();
        }

        //public async Task<IEnumerable<UnitModel>> GetDeffenceUnits(Tribe tribe)
        //{
        //    var units = await collection
        //        .AsQueryable()
        //        .Where(x => x.Tribe == tribe && (x.UnitType == UnitType.FOOT_TROOPS || x.UnitType == UnitType.CAVALRY))
        //        .ToListAsync();

        //    return units.Where(x => x.Attack < x.DeffenceAgainstCavalry || x.Attack < x.DeffenceAgainstInfantry);
        //}

        public async Task<IEnumerable<UnitModel>> GetDeffenceUnits(Tribe tribe)
        {
            PipelineDefinition<UnitModel, UnitModel> pipeline = new BsonDocument[]
            {
                new BsonDocument{
                    { "$addFields", new BsonDocument
                        {
                            { "iDiff", new BsonDocument { { "$cmp", new BsonArray { "$attack", "$deffenceAgainstInfantry" } } } },
                            { "cDiff", new BsonDocument { { "$cmp", new BsonArray { "$attack", "$deffenceAgainstCavalry" } } } }
                        }
                    }
                },
                new BsonDocument
                {
                    {
                        "$match", new BsonDocument
                        {
                            {
                                "$and", new BsonArray
                                {
                                    new BsonDocument
                                    {
                                        { "tribe", tribe.GetEnumDisplayName() }
                                    },
                                    new BsonDocument
                                    {
                                        {
                                            "$or", new BsonArray
                                            {
                                                new BsonDocument
                                                {
                                                    { "iDiff", new BsonDocument { { "$eq", -1 } } }
                                                },
                                                new BsonDocument
                                                {
                                                    { "cDiff", new BsonDocument { { "$eq", -1 } } }
                                                }
                                            }
                                        }
                                    },
                                    new BsonDocument
                                    {
                                        {
                                            "$or", new BsonArray
                                            {
                                                new BsonDocument
                                                {
                                                    { "unitType", UnitType.FOOT_TROOPS.GetEnumDisplayName() }
                                                },
                                                new BsonDocument
                                                {
                                                    { "unitType", UnitType.CAVALRY.GetEnumDisplayName() }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var units = _collection.Aggregate(pipeline);
            return await units.ToListAsync();
        }

        public async Task<IEnumerable<UnitModel>> GetOffenceUnits(Tribe tribe)
        {
            var units = await _collection
                .AsQueryable()
                .Where(x => x.Tribe == tribe && (x.UnitType == UnitType.FOOT_TROOPS || x.UnitType == UnitType.CAVALRY || x.UnitType == UnitType.WAR_MACHINE))
                .ToListAsync();

            return units.Where(x => x.Attack > x.DeffenceAgainstCavalry || x.Attack > x.DeffenceAgainstInfantry || x.UnitType == UnitType.WAR_MACHINE);
        }

        public async Task<UnitModel> GetScanUnit(Tribe tribe)
        {
            var unit = await _collection
                   .AsQueryable()
                   .FirstOrDefaultAsync(x => x.Tribe == tribe && x.UnitType == UnitType.SCOUT);

            return unit;
        }

        public async Task<UnitModel> GetTrader(Tribe tribe)
        {
            var unit = await _collection
                      .AsQueryable()
                      .FirstOrDefaultAsync(x => x.Tribe == tribe && x.Name == "trader");

            return unit;
        }

        public async Task<UnitModel> GetUnit(string name, Tribe tribe)
        {
            var unit = await _collection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Tribe == tribe && x.Name == name);

            return unit;
        }

        public async Task<IEnumerable<UnitModel>> GetUnitsByTribe(Tribe tribe)
        {
            var units = await _collection
                   .AsQueryable()
                   .Where(x => x.Tribe == tribe)
                   .ToListAsync();

            return units;
        }

        public async Task<IEnumerable<UnitModel>> GetUnitsByType(Tribe tribe, UnitType type)
        {
            var filter = Builders<UnitModel>.Filter.And(new[] {
                Builders<UnitModel>.Filter.Eq("tribe", tribe),
                Builders<UnitModel>.Filter.Eq("unitType", type)
            });

            return await base.Get(filter);
        }
    }
}
