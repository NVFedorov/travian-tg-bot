using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;

namespace TravianTelegramBot.ViewModels
{
    public class VillageViewModel
    {
        public string PlayerName { get; set; }
        public string VillageId { get; set; }
        public string VillageName { get; set; }
        public bool IsSaveResourcesFeatureOn { get; set; }
        public bool IsSaveTroopsFeatureOn { get; set; }
        public bool IsSpamFeatureOn { get; set; }
        public bool IsBuildFeatureOn { get; set; }
        public bool IsOffence { get; set; }
        public bool IsDeffence { get; set; }
        public bool IsResource { get; set; }
        public bool IsScan { get; set; }
        public bool IsUnderAttack { get; set; }
        public int Warehourse { get; set; }
        public int Granary { get; set; }
        public ResourcesModel Resources { get; set; }
        public ResourcesModel ResourcesProduction { get; set; }
        public List<string> PreferableUnits { get; set; }
        public Dictionary<string, int> SpamUnits { get; set; }
        public DateTimeOffset? NextBuildingPlanExecutionTime { get; set; }
        public bool IsWaitingForResources { get; set; }
        public string BuildingPlanId { get; set; }
    }
}
