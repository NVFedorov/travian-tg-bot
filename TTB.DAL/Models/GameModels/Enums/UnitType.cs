using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TTB.DAL.Models.GameModels.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UnitType
    {
        [Display(Name = "FOOT_TROOPS")]
        FOOT_TROOPS,
        [Display(Name = "CAVALRY")]
        CAVALRY,
        [Display(Name = "SCOUT")]
        SCOUT,
        [Display(Name = "WAR_MACHINE")]
        WAR_MACHINE,
        [Display(Name = "EXPANSION")]
        EXPANSION
    }
}
