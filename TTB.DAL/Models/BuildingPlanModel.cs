using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels.Enums;

namespace TTB.DAL.Models
{
    [BsonIgnoreExtraElements]
    public class BuildingPlanModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Name { get; set; }
        public string BotUserName { get; set; }
        public VillageType VillageType { get; set; }
        public List<BuildingPlanStepModel> BuildingSteps { get; set; }
    }
}
