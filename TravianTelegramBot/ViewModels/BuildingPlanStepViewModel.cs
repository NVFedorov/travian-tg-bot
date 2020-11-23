using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;

namespace TravianTelegramBot.ViewModels
{
    public class BuildingPlanStepViewModel
    {
        public int Order { get; set; }
        public BuildingModel Building { get; set; }
        public int Level { get; set; }
    }
}
