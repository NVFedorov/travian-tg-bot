using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace TTB.DAL.Models.ScenarioModels
{
    [BsonIgnoreExtraElements]
    public class VillageStatus
    {
        [BsonElement("isUnderAttack")]
        [JsonProperty("isUnderAttack")]
        public bool IsUnderAttack { get; set; }
        [BsonElement("attacks")]
        [JsonProperty("attacks")]
        public IEnumerable<DateTimeOffset> Attacks { get; set; }
    }
}
