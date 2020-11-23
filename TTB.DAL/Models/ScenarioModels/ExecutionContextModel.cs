using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.PlayerModels.Enums;

namespace TTB.DAL.Models.ScenarioModels
{
    [BsonIgnoreExtraElements]
    public class ExecutionContextModel
    {
        public bool IsUserNotified { get; set; }
        public List<CommandModel> Commands { get; set; }
    }
}
