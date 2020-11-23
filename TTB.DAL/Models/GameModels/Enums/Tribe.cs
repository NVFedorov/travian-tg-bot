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
    public enum Tribe
    {
        [Display(Name = "NOT_SPECIFIED")]
        NOT_SPECIFIED,
        [Display(Name = "ROMAN")]
        ROMAN,
        [Display(Name = "GAUL")]
        GAUL,
        [Display(Name = "TEUTON")]
        TEUTON,
        [Display(Name = "NATURE")]
        NATURE,
        [Display(Name = "NATAR")]
        NATAR
    }
}
