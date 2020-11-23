using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Models.PlayerModels
{
    public class VillageModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string InternalId { get; set; }
        public string PlayerName { get; set; }
        public string VillageId { get; set; }
        [BsonRequired]
        public string VillageName { get; set; }
        public string Alliance { get; set; }
        public Tribe Tribe { get; set; }
        [BsonRequired]
        public int CoordinateX { get; set; }
        [BsonRequired]
        public int CoordinateY { get; set; }
        [BsonIgnoreIfNull]
        public bool IsCapital { get; set; }
        [BsonIgnoreIfNull]
        public bool IsSaveResourcesFeatureOn { get; set; }
        [BsonIgnoreIfNull]
        public bool IsSaveTroopsFeatureOn { get; set; }
        [BsonIgnoreIfNull]
        public bool IsSpamFeatureOn { get; set; }
        [BsonIgnoreIfNull]
        public bool IsBuildFeatureOn { get; set; }
        [BsonIgnoreIfNull]
        public IEnumerable<VillageType> Types { get; set; }
        public int Warehourse { get; set; }
        public int Granary { get; set; }
        public ResourcesModel Resources { get; set; }
        public ResourcesModel ResourcesProduction { get; set; }
        public List<AttackModel> Attacks { get; set; }
        public Dictionary<string, int> Units { get; set; }
        public List<string> PreferableUnits { get; set; }
        public IDictionary<string, int> SpamUnits { get; set; }
        public IDictionary<string, int> Buildings { get; set; }
        [BsonIgnoreIfNull]
        public DateTimeOffset? NextBuildingPlanExecutionTime { get; set; }
        public bool IsWaitingForResources { get; set; }
        public string BuildingPlanId { get; set; }
        // TODO: units levels
    }
}
