using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels.Enums;

namespace TravianTelegramBot.ViewModels
{
    public class BuildingPlanViewModel
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string BotUserName { get; set; }
        [EnumDataType(typeof(VillageType))]
        public VillageType VillageType { get; set; }
        public List<BuildingPlanStepViewModel> BuildingSteps { get; set; }
    }
}
