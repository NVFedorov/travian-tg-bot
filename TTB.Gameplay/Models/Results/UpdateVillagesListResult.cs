using System;
using System.Collections.Generic;
using System.Text;
using TTB.Gameplay.Models.ContextModels;

namespace TTB.Gameplay.Models.Results
{
    public class UpdateVillagesListResult
    {
        public List<Village> Villages { get; set; }
        public Tribe Tribe { get; set; }
    }
}
