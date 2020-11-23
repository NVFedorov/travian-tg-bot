using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels.Enums;

namespace TTB.DAL.Models.PlayerModels
{
    [BsonIgnoreExtraElements]
    public class PlayerDataModel
    {
        public string UserName { get; set; }
        [EnumDataType(typeof(PlayerStatus))]
        public PlayerStatus Status { get; set; }
        [EnumDataType(typeof(Tribe))]
        public Tribe Tribe { get; set; }
        public string Alliance { get; set; }
        public string TimeZone { get; set; }
        public string WatchCron { get; set; }
        public List<string> VillagesIds { get; set; }
    }
}
