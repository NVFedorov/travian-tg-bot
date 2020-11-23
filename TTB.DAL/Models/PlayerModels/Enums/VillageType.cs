using System.ComponentModel.DataAnnotations;

namespace TTB.DAL.Models.PlayerModels.Enums
{
    public enum VillageType
    {
        [Display(Name = "RESOURCES")]
        RESOURCES,
        [Display(Name = "DEFFENCE")]
        DEFFENCE,
        [Display(Name = "SCAN")]
        SCAN,
        [Display(Name = "OFFENCE")]
        OFFENCE,
    }
}
