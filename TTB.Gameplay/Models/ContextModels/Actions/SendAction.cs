using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public abstract class SendAction : GameAction
    {
        public Village To { get; set; }
    }
}
