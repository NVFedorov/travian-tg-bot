using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class ParseSpamTargetsScenario : BaseScenario
    {
        public ParseSpamTargetsScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            _driver.Navigate().GoToUrl(context.StartUrl);

            //var header = _driver.FindElement(By.CssSelector(".header.text")).Text.ToLower();
            //if (header.Contains("spam") || header.Contains("спам"))
            //{

            //}

            var targets = _driver.FindElements(By.CssSelector("#message a.bbCoordsLink")).Select(x => x.GetAttribute("href")).ToList();
            var result = new BaseScenarioResult();
            foreach(var target in targets)
            {
                var village = ParseVillageDetailsFromUrl(target);
                result.Villages.Add(village);
            }

            return result;
        }
    }
}
