using System.ComponentModel.DataAnnotations;

namespace TTB.DAL.Models.ScenarioModels.Enums
{
    public enum WatchStatus
    {
        [Display(Name = "DEFAULT")]
        DEFAULT,
        [Display(Name = "RUNNING")]
        RUNNING,
        [Display(Name = "WAITING")]
        WAITING,
        [Display(Name = "ATTACK_ALERT")]
        ATTACK_ALERT,
        [Display(Name = "SCAN_ALERT")]
        SCAN_ALERT
    }
}
