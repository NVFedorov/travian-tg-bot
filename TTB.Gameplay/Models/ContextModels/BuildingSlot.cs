using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public enum BuildingSlotState
    {
        Empty,
        NotNow,
        Good,
        MaxLevel,
        UnderConstruction,
        Gray
    }

    public class BuildingSlot
    {
        public string Id { get; set; }
        public string BuildingId { get; set; }
        public int Level { get; set; }
        public BuildingSlotState State { get; set; }
    }
}
