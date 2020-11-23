using System;
using System.Collections.Generic;
using System.Text;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public class SendArmyAction : SendAction
    {
        public SendArmyType Type { get; set; }
        public Dictionary<string, int> UnitsToSend { get; set; }
        public bool SendAll { get; set; }
        public List<string> CatapultsTargets { get; set; }
    }
}
