using System;
using System.Collections.Generic;
using System.Text;
using TTB.Gameplay.Models.ContextModels;

namespace TTB.Gameplay.Models.ContextModels
{
    public class Incoming
    {
        public string VillageName { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public Village IntruderDetails { get; set; }
        public string IntruderVillageUrl { get; set; }
        public List<string> PossibleUnits { get; set; }
    }
}
