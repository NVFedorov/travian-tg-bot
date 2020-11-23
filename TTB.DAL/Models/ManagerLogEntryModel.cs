using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTB.DAL.Models
{
    [BsonIgnoreExtraElements]
    public class ManagerLogEntryModel : LogEntryModel
    {
        [BsonElement("MessageTemplate")]
        public string MessageTemplate { get; set; }
        [BsonElement("UtcTimestamp")]
        public DateTime UtcTimestamp { get; set; }
    }
}
