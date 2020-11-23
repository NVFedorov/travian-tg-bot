using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using TTB.Common.Extensions;
using TTB.Common.Settings;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class IncomingAttacksScenario : CommonScenario
    {
        public IncomingAttacksScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override Village ExecuteInVillage(ScenarioContext context, Village source)
        {
            source.Attacks = CreateIncomingAttacksReport(context, source, "1", "1");
            source.IsUnderAttack = source.Attacks.Any();
            return source;
        }
        
        private List<Incoming> CreateIncomingAttacksReport(ScenarioContext context, Village village, string filter, string subfilter)
        {
            // go to troops center -> review -> incoming troops -> filter by incoming attack
            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/build.php?gid=16&tt=1&filter=" + filter + "&subfilters=" + subfilter));

            var pages = _driver.FindElements(By.CssSelector("div.paginatorTop div.paginator .number"));
            var result = new List<Incoming>();
            for (int i = 1; i < pages.Count + 1; i++)
            {
                this._driver.Navigate().GoToUrl(new Uri(
                    context.Player.Uri,
                    "/build.php?gid=16&tt=1&filter=" + filter + "&subfilters=" + subfilter + "&page=" + i));

                var troopsMovementsTables = _driver.FindElements(By.CssSelector("div#build.gid16 table.troop_details"));
                if (troopsMovementsTables == null || troopsMovementsTables.Count == 0)
                {
                    continue;
                }

                foreach (var table in troopsMovementsTables)
                {
                    var timer = int.Parse(table.FindElement(By.CssSelector("div.in .timer")).GetAttribute("value"));
                    var at = table.FindElement(By.CssSelector("div.at span")).Text;
                    at = at.Substring(at.Length - 8);

                    var localTime = at.ToTimeSpan();
                    var arrivalFromTimer = DateTimeOffset.UtcNow.AddSeconds(timer) + context.Player.TimeZone.ToOffset();
                    var arrival = new DateTimeOffset(arrivalFromTimer.Year, arrivalFromTimer.Month, arrivalFromTimer.Day, 0, 0, 0, context.Player.TimeZone.ToOffset());
                    arrival += localTime;

                    var mapUrl = table.FindElement(By.CssSelector("td.role a")).GetAttribute("href");
                    var incoming = new Incoming
                    {
                        DateTime = arrival.ToUniversalTime(),
                        VillageName = village.Name,
                        IntruderVillageUrl = mapUrl
                    };
                    result.Add(incoming);
                }
            }

            foreach(var v in result)
            {
                v.IntruderDetails = this.ParseVillageDetailsFromUrl(v.IntruderVillageUrl);
            }

            return result;
        }
    }
}
