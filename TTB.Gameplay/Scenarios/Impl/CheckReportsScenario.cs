using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TTB.Common.Extensions;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class CheckReportsScenario : BaseScenario
    {
        public CheckReportsScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            var newReports = this._driver.FindElements(By.CssSelector("#n5.reports div.speechBubbleContainer"));
            if (newReports.Any())
            {
                this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/berichte.php?t=3&opt=AAATABIA"));
                var reportsForm = this._driver.FindElement(By.Id("reportsForm"));
                var anchors = reportsForm.FindElements(By.CssSelector("table tbody tr td.sub.newMessage div a"));
                var links = new List<string>();

                foreach (var anchor in anchors)
                {
                    links.Add(anchor.GetAttribute("href"));
                }

                foreach (var link in links)
                {
                    this._driver.Navigate().GoToUrl(link);
                    var villageElem = this._driver.FindElement(By.CssSelector("#reportWrapper div.role.attacker div.troopHeadline a.village"));
                    var userVillageElem = this._driver.FindElement(By.CssSelector("#reportWrapper div.role.defender div.troopHeadline a.village"));
                    var timeElem = this._driver.FindElement(By.CssSelector("#reportWrapper div.time div.text"));
                    var villageName = userVillageElem.Text;
                    var villageUrl = villageElem.GetAttribute("href");

                    var scan = new Incoming
                    {
                        VillageName = villageName,
                        DateTime = timeElem.Text.ToDateTimeOffset(),
                        IntruderVillageUrl = villageUrl
                    };

                    result.Scans.Add(scan);
                }

                foreach(var scan in result.Scans)
                {
                    scan.IntruderDetails = ParseVillageDetailsFromUrl(scan.IntruderVillageUrl);
                }
            }

            if (!result.Scans.Any())
            {
                result.Messages.Add("No scan events found");
            }

            return result;
        }
    }
}
