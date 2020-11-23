using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Models
{
    [BsonIgnoreExtraElements]
    public class TravianUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string InternalId { get; set; }
        [BsonElement("userName")]
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("password")]
        [BsonElement("password")]
        public string Password { get; set; }
        [JsonProperty("url")]
        [BsonElement("url")]
        public string Url { get; set; }
        //[JsonProperty("timeZoneId")]
        //[BsonElement("timeZoneId")]
        //public string TimeZoneId { get; set; }
        [JsonProperty("cookies")]
        [BsonElement("cookies")]
        public IEnumerable<CookieModel> Cookies { get; set; }
        [JsonProperty("isActive")]
        [BsonElement("isActive")]
        public bool IsActive { get; set; }
        [JsonProperty("botUserName")]
        [BsonElement("botUserName")]
        public string BotUserName { get; set; }
        [JsonProperty("executionContext")]
        [BsonElement("executionContext")]
        public ExecutionContextModel ExecutionContext { get; set; }
        [JsonProperty("playerData")]
        [BsonElement("playerData")]
        public PlayerDataModel PlayerData { get; set; }
    }
}
