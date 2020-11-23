using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels.Enums;

namespace TTB.DAL.Models.GameModels
{
    public class BuildingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        [BsonRequired]
        public string Name { get; set; }
        [BsonRequired]
        public string BuildingId { get; set; }
        public string PrefferedBuildingSlot { get; set; }
        public string LocalizedNameEn { get; set; }
        public string LocalizedNameRu { get; set; }
        public LevelInfoModel[] LevelInfos { get; set; }
        public PrerequiresiteModel[] Prerequiresites { get; set; }
        public Tribe Tribe { get; set; }
    }

    public class LevelInfoModel
    {
        public int Level { get; set; }
        public ResourcesModel Resources { get; set; }
        public TimeSpan BuildingTime { get; set; }
        public int CultureProduction { get; set; }
        public int CropConsumption { get; set; }
        public double UpgradedProperty { get; set; }
    }

    public class PrerequiresiteModel
    {
        public string Name { get; set; }
        public int Level { get; set; }
    }
}
