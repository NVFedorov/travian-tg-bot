using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TTB.DAL.Models
{
    [BsonIgnoreExtraElements]
    public class LogEntryModel
    {
        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
        [BsonElement("Level")]
        public string Level { get; set; }
        [BsonElement("RenderedMessage")]
        public string RenderedMessage { get; set; }
        [BsonElement("Exception")]
        public string Exception { get; set; }
    }
}
