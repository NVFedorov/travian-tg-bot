using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public class Village
    {
        public Village()
        {
            Attacks = new List<Incoming>();
            CanBuild = true;
        }

        public string Name { get; set; }
        public string PlayerName { get; set; }
        public string Alliance { get; set; }
        public string Url { get; set; }
        public int CoordinateX { get; set; }
        public int CoordinateY { get; set; }
        public Resources Resources { get; set; }
        public Resources ResourcesProduction { get; set; }
        public int Warehourse { get; set; }
        public int Granary { get; set; }
        public int Stash { get; set; }
        public bool IsUnderAttack { get; set; }
        public bool IsCapital { get; set; }
        public List<Incoming> Attacks { get; set; }

        // Unit ID / Quantity
        public Dictionary<string, int> Units { get; set; }
        
        public List<BuildingSlot> BuildingSlots { get; set; }
        public TimeSpan? Dorf1BuildTimeLeft { get; set; }
        public TimeSpan? Dorf2BuildTimeLeft { get; set; }
        public bool CanBuild { get; set; }
        public string BuildingPlanId { get; set; }
    }
}
