using System.Collections.Generic;
using OpenQA.Selenium;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;

namespace TTB.Gameplay.Models
{
    public class ScenarioContext
    {
        public Player Player { get; set; }
        public IEnumerable<Cookie> Cookies { get; set; }
        public IEnumerable<GameAction> Actions { get; set; }
        public string StartUrl { get; set; }
    }
}
