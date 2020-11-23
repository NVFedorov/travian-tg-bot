using System.ComponentModel.DataAnnotations;

namespace TTB.DAL.Models.PlayerModels.Enums
{
    public enum PlayerStatus
    {
        [Display(Name = "ALL_QUIET")]
        ALL_QUIET,
        [Display(Name = "UNDER_ATTACK")]
        UNDER_ATTACK,
        [Display(Name = "WAS_SCANNED")]
        WAS_SCANNED
    }
}
