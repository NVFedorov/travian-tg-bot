using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace TTB.DAL.Models.ScenarioModels
{
    [BsonIgnoreExtraElements]
    public class ScanNotificationModel
    {
        [BsonElement("scanId")]
        [JsonProperty("scanId")]
        public string ScanId { get; set; }
        [BsonElement("userName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [BsonElement("userVillageName")]
        [JsonProperty("userVillageName")]
        public string UserVillageName { get; set; }
        [BsonElement("dateTime")]
        [JsonProperty("dateTime")]
        public string DateTime { get; set; }
        [BsonElement("wasShown")]
        [JsonProperty("wasShown")]
        public bool WasShown { get; set; }
        [BsonElement("details")]
        [JsonProperty("details")]
        public VillageDetailsModel Details { get; set; }
    }
}
