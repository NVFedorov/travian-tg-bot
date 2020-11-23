using System.ComponentModel.DataAnnotations;

namespace TTB.DAL.Models.ScenarioModels.Enums
{
    public enum ArmyEventType
    {
        [Display(Name = "INCOMING_ATTACK")]
        INCOMING_ATTACK,
        [Display(Name = "OUTGOING_ATTACK")]
        OUTGOING_ATTACK,
        [Display(Name = "RETURNING")]
        RETURNING,
        [Display(Name = "OUTGOING_DEFFENCE")]
        OUTGOING_DEFFENCE,
        [Display(Name = "INCOMING_DEFFENCE")]
        INCOMING_DEFFENCE,
        [Display(Name = "SCAN")]
        SCAN
    }
}
