using System;
using System.Collections.Generic;
using System.Text;
using TTB.DAL.Models.PlayerModels;

namespace TTB.DAL.Models.ScenarioModels
{
    public class AttackModel
    {
        public string VillageName { get; set; }
        public VillageModel FromVillage { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string IntruderVillageUrl { get; set; }
        public List<string> PossibleUnits { get;set; }
    }
}
