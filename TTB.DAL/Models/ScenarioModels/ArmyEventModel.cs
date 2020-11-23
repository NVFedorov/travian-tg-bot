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
    public class ArmyEventModel
    {
        [BsonElement("eventId")]
        [JsonProperty("eventId")]
        public string EventId { get; set; }
        [BsonElement("armyEventId")]
        [JsonProperty("armyEventId")]
        public string ArmyEventId { get; set; }
        [BsonElement("userName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [BsonElement("userVillage")]
        [JsonProperty("userVillage")]
        public string UserVillage { get; set; }
        [BsonElement("dateTime")]
        [JsonProperty("dateTime")]
        public string DateTime { get; set; }
        [BsonElement("eventType")]
        [JsonProperty("eventType")]
        public ArmyEventType EventType { get; set; }
        [BsonElement("importance")]
        [JsonProperty("importance")]
        public int Importance { get; set; }
        [BsonElement("details")]
        [JsonProperty("details")]
        public VillageDetailsModel Details { get; set; }
        [BsonElement("detailsUrl")]
        [JsonProperty("detailsUrl")]
        public string DetailsUrl { get; set; }
        [BsonElement("isNew")]
        [JsonProperty("isNew")]
        public bool IsNew { get; set; }
        [BsonElement("arePreparationsDone")]
        [JsonProperty("arePreparationsDone")]
        public bool ArePreparationsDone { get; set; }
    }
}
