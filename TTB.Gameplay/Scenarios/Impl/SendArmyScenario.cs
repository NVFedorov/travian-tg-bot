using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class SendArmyScenario : VillageScenario<SendArmyAction>
    {
        public SendArmyScenario(IWebDriverProvider driverProvider, SendArmyAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            if (_action.To.Name == "send_to_any_natar")
            {
                this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri + "/build.php?tt=99&id=39"));
                var natarRows = _driver.FindElements(By.XPath($"//tr[./td[@class='village' and ./a[text()[contains(., 'Натары')]]]]")).ToList();
                var min = natarRows.OrderBy(x => double.Parse(x.FindElement(By.ClassName("distance")).Text)).FirstOrDefault();

                var natars = min.FindElements(By.TagName("a")); // "//tr[//a[contains(text(), 'Натары')]]"
                if (natars.Any())
                {
                    natars.First().Click();
                    this._driver.FindElement(By.XPath("//a[contains(text(), 'Отправить войска')]")).Click();
                }
            }
            else
            {
                // go to army center tab 2 (send army)
                this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri + "/build.php?tt=2&id=39"));
                _driver.FindElement(By.Id("xCoordInput")).SendKeys(_action.To.CoordinateX.ToString());
                _driver.FindElement(By.Id("yCoordInput")).SendKeys(_action.To.CoordinateY.ToString());
            }

            var options = _driver.FindElements(By.CssSelector("form div.option label"));
            if (options.Count != 3)
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = "Can't find options to send army",
                    ErrorSource = nameof(SendArmyScenario)
                });
            }
            else
            {
                switch (_action.Type)
                {
                    case SendArmyType.BACKUP:
                        options[0].Click();
                        break;
                    case SendArmyType.ATTACK:
                        options[1].Click();
                        break;
                    case SendArmyType.RAID:
                        options[2].Click();
                        break;
                    default:
                        throw new Exception("Unknown send army type");
                }
            }

            if (_action.SendAll)
            {
                var links = _driver.FindElements(By.CssSelector("table#troops td a"));
                foreach(var l in links)
                {
                    l.Click();
                }
            }
            else
            {
                var table = _driver.FindElement(By.Id("troops"));
                foreach (var units in _action.UnitsToSend)
                {
                    var tds = table.FindElements(By.XPath($"//td[img[@alt='{units.Key}']]"));
                    if (tds.Any())
                    {
                        var td = tds.First();
                        var a = td.FindElements(By.TagName("a"));
                        if (!a.Any())
                            continue;

                        if (units.Value == -1)
                        {
                            td.FindElement(By.TagName("a")).Click();
                        }
                        else
                        {
                            td.FindElement(By.TagName("input")).SendKeys(units.Value.ToString());
                        }
                    }
                }
            }

            // send
            _driver.FindElement(By.Id("btn_ok")).Click();

            // if no troops choosen
            var errors = _driver.FindElements(By.ClassName("error"));
            if (errors.Any())
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = "no troops choosen",
                    ErrorSource = nameof(SendArmyScenario)
                });
            }
            else
            {
                // confirm
                _driver.FindElement(By.Id("btn_ok")).Click();
                result.Villages.Add(_action.To);
            }
            return result;
        }
    }
}
