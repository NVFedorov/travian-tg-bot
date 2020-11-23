using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class TrainArmyScenario : VillageScenario<TrainArmyAction>
    {
        public TrainArmyScenario(IWebDriverProvider driverProvider, TrainArmyAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/dorf2.php"));
            this._driver.FindElement(By.CssSelector("#village_map .g19 .level")).Click();
            var result = new BaseScenarioResult();
            foreach (var unit in this._action.UnitsToTrain)
            {
                var unitWrappers = this._driver.FindElements(By.XPath("//div[contains(@class, 'innerTroopWrapper') and .//a[text() = \'" + unit.Key + "\']]"));
                if (unitWrappers != null && unitWrappers.Any())
                {
                    if (unit.Value == (int)TrainArmyFlag.MAX)
                    {
                        var a = unitWrappers[0].FindElement(By.CssSelector("div.cta > a"));
                        a.Click();
                    }
                    else if (unit.Value > 0)
                    {
                        var input = unitWrappers[0].FindElement(By.CssSelector("input.text"));
                        input.SendKeys(unit.Value.ToString());
                    }

                    this._driver.FindElement(By.Id("s1")).Click();
                    result.Messages.Add($"Started to train {unit.Value} units of {unit.Key}");
                }
                else
                {
                    result.Errors.Add(new ScenarioError
                    {
                        ErrorMessage = $"Unable to find unit {unit.Key}",
                        ErrorSource = nameof(TrainArmyScenario),
                        ErrorType = "NotFound"
                    });
                }
            }

            return result;
        }
    }
}
