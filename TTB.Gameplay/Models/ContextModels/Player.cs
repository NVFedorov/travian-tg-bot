using System;
using System.Collections.Generic;
using System.Text;

namespace TTB.Gameplay.Models.ContextModels
{
    public class Player
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Uri Uri { get; set; }
        public Tribe Tribe { get; set; }
        public string TimeZone { get; set; }
        public string Alliance { get; set; }
    }
}
