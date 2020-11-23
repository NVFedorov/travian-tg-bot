using System.Collections.Generic;

using Newtonsoft.Json;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels.Enums;

namespace TTB.DAL.Models.ScenarioModels
{
    public class ActionModel
    {
        [JsonProperty("from")]
        public VillageModel From { get; set; }
        [JsonProperty("to")]
        public VillageModel To { get; set; }
        [JsonProperty("action")]
        public ActionType Action { get; set; }
        [JsonProperty("parameters")]
        public IEnumerable<Parameter> Parameters { get; set; }
    }
}
