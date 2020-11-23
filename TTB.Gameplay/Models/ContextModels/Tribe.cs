using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public enum Tribe
    {
        [Display(Name = "UNDEFINED")]
        UNDEFINED,
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
