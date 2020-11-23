using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.ScenarioModels.Enums;

namespace TTB.DAL.Models.ScenarioModels
{

    [BsonIgnoreExtraElements]
    public class IncomingAttackNotificationModel
    {
        [BsonElement("userName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [BsonElement("villageName")]
        [JsonProperty("villageName")]
        public string VillageName { get; set; }
        [BsonElement("nearestAttackDateTime")]
        [JsonProperty("nearestAttackDateTime")]
        public string NearestAttackDateTime { get; set; }
        [BsonElement("createdDateTime")]
        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }
        [BsonElement("attacksNumber")]
        [JsonProperty("attacksNumber")]
        public int AttacksNumber { get; set; }
        [BsonElement("status")]
        [JsonProperty("status")]
        public WatchStatus Status { get; set; }
    }
}
