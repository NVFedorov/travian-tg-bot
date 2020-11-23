using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public class Resources
    {        
        public int Lumber { get; set; }
        public int Clay { get; set; }
        public int Iron { get; set; }
        public int Crop { get; set; }
        public int FreeCrop { get; set; }

        public List<int> ToList()
        {
            return new List<int>
            {
                Lumber,
                Clay,
                Iron,
                Crop,
                FreeCrop
            };
        }
    }
}
