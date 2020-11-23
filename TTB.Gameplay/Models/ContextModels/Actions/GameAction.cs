using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public class GameAction
    {
        public GameActionType Action { get; set; }
        public Village Village { get; set; }
    }
}
