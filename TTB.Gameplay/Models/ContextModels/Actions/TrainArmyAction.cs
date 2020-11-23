using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public class TrainArmyAction : GameAction
    {
        public Dictionary<string, int> UnitsToTrain { get; set; }
    }
}
