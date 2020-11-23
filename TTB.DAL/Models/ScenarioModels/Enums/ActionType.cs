using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TTB.DAL.Models.ScenarioModels.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionType
    {
        [Display(Name = "SEND_RESOURCES")]
        SEND_RESOURCES,
        [Display(Name = "SEND_ARMY")]
        SEND_ARMY,
        [Display(Name = "TRAIN_ARMY")]
        TRAIN_ARMY,
        [Display(Name = "UPGRADE_ARMY")]
        UPGRADE_ARMY,
        [Display(Name = "BUILD")]
        BUILD
    }
}
