using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public class SendWavesAction : SendArmyAction
    {
        public int Waves { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
