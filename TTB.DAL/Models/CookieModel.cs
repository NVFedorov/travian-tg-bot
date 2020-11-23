using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTB.DAL.Models
{
    public class CookiesViewModel
    {
        [JsonProperty("cookies")]
        public List<CookieModel> Cookies { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CookieModel
    {
        [JsonProperty("name")]
        [BsonElement("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        [BsonElement("value")]
        public string Value { get; set; }
        [JsonProperty("domain")]
        [BsonElement("domain")]
        public string Domain { get; set; }
        [JsonProperty("path")]
        [BsonElement("path")]
        public string Path { get; set; }
        [JsonProperty("expiry")]
        [BsonElement("expiry")]
        public string Expiry { get; set; }
        [JsonProperty("isSecure")]
        [BsonElement("isSecure")]
        public bool IsSecure { get; set; }
        [JsonProperty("isHttpOnly")]
        [BsonElement("isHttpOnly")]
        public bool IsHttpOnly { get; set; }
    }
}
