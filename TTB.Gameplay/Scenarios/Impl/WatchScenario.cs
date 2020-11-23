using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using TTB.Common.Extensions;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class WatchScenario : CommonScenario
    {
        public WatchScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override Village ExecuteInVillage(ScenarioContext context, Village source)
        {
            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/dorf1.php"));
            var attackElem = this._driver.FindElements(By.XPath("//table[@id='movements']//tr[td/a/img[@class='att1']]/td[div[@class='mov']]"));
            source.IsUnderAttack = attackElem.Any();
            if (source.IsUnderAttack)
            {
                var elem = attackElem.First();
                //var mov = elem.FindElement(By.CssSelector("span.a1"));
                //int attacksNumber = int.Parse(mov.Text.Split(" ")[0]);
                var dur = elem.FindElement(By.CssSelector("span.timer"));
                int timer = int.Parse(dur.GetAttribute("value"));

                var nearest = DateTimeOffset.UtcNow.AddSeconds(timer);
                source.Attacks.Add(new Incoming
                {
                    VillageName = source.Name,
                    DateTime = nearest
                });
            }

            // update available resources, storage and stash info
            return source;
        }
    }
}
