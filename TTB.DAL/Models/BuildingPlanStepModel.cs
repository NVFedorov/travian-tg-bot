using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;

namespace TTB.DAL.Models
{
    public class BuildingPlanStepModel
    {
        public int Order { get; set; }
        public string BuildingId { get; set; }
        public int Level { get; set; }
    }
}
