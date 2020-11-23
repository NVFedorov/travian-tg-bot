using System.Collections.Generic;

using MongoDB.Bson.Serialization.Attributes;

using Newtonsoft.Json;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels.Enums;

namespace TTB.DAL.Models.ScenarioModels
{
    [BsonIgnoreExtraElements]
    public class VillageDetailsModel
    {
        [BsonElement("villageId")]
        [JsonProperty("villageId")]
        public string VillageId { get; set; }
        [BsonElement("player")]
        [JsonProperty("player")]
        public string Player { get; set; }
        [BsonElement("village")]
        [JsonProperty("village")]
        public string Village { get; set; }
        [BsonElement("alliance")]
        [JsonProperty("alliance")]
        public string Alliance { get; set; }
        [BsonElement("villageUrl")]
        [JsonProperty("villageUrl")]
        public string VillageUrl { get; set; }
        [BsonElement("coordinateX")]
        [JsonProperty("coordinateX")]
        public int CoordinateX { get; set; }
        [BsonElement("coordinateY")]
        [JsonProperty("coordinateY")]
        public int CoordinateY { get; set; }
        [BsonElement("isCapital")]
        [JsonProperty("isCapital")]
        public bool IsCapital { get; set; }
        [BsonElement("isSaveResourcesFeatureOn")]
        [JsonProperty("isSaveResourcesFeatureOn")]
        public bool IsSaveResourcesFeatureOn { get; set; }
        [BsonElement("isSaveTroopsFeatureOn")]
        [JsonProperty("isSaveTroopsFeatureOn")]
        public bool IsSaveTroopsFeatureOn { get; set; }
        [BsonElement("types")]
        [JsonProperty("types")]
        public IEnumerable<VillageType> Types { get; set; }
        [BsonElement("storage")]
        [JsonProperty("storage")]
        public StorageModel Storage { get; set; }
        [BsonElement("stash")]
        [JsonProperty("stash")]
        public ResourcesModel Stash { get; set; }
        [BsonElement("resourcesProduction")]
        [JsonProperty("resourcesProduction")]
        public ResourcesModel ResourcesProduction { get; set; }
        [BsonElement("status")]
        [JsonProperty("status")]
        public VillageStatus Status { get; set; }
    }
}
