using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels.Actions
{
    public class BuildAction : GameAction
    {
        public string BuildingId { get; set; }
        public string BuildingSlot { get; set; }
        public int Level { get; set; }
    }
}
