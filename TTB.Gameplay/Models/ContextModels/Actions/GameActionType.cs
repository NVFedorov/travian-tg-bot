using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public enum GameActionType
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
