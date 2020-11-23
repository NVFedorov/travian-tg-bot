using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels;

namespace TTB.DAL.Models.ScenarioModels
{
    public class StorageModel : ResourcesModel
    {
        public int Gold { get; set; }
    }
}
