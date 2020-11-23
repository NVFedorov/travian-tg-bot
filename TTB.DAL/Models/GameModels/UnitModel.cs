using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using TTB.DAL.Models.GameModels.Enums;

namespace TTB.DAL.Models.GameModels
{
    [BsonIgnoreExtraElements]
    public class UnitModel
    {
        public string Name { get; set; }
        public string LocalizedNameEn { get; set; }
        public string LocalizedNameRu { get; set; }
        public Tribe Tribe { get; set; }
        public string Description { get; set; }
        public double Expenses { get; set; }
        public double Speed { get; set; }
        public double Capacity { get; set; }
        public double? Attack { get; set; }
        public double? DeffenceAgainstInfantry { get; set; }
        public double? DeffenceAgainstCavalry { get; set; }
        public UnitType UnitType { get; set; }
        public ResourcesModel TrainingCost { get; set; }
        public ResourcesModel ResearchCost { get; set; }
        public TimeSpan ResearchTime { get; set; }
        public TimeSpan TrainingTime { get; set; }
    }
}
