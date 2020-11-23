using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TTB.DAL.Models
{
    public class SecretModel
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("value")]
        public string Value { get; set; }
        [BsonElement("expiry")]
        public DateTime Expiry { get; set; }
        [BsonElement("adminPrivileges")]
        public bool AdminPrivileges { get; set; }
    }
}
