using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class SendWavesScenario : VillageScenario<SendWavesAction>
    {
        public SendWavesScenario(IWebDriverProvider driverProvider, SendWavesAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            var windows = new List<string>();

            var uri = new Uri(context.Player.Uri + "/build.php?tt=2&id=39");
            for (int i = 0; i < _action.Waves; i++)
            {
                if (i > 0)
                {
                    OpenNewTab(uri.AbsoluteUri);
                    WaitUntilElementExists(By.Id("xCoordInput"));
                }
                else
                {
                    _driver.Navigate().GoToUrl(uri);
                }

                // go to army center tab 2 (send army)
                _driver.FindElement(By.Id("xCoordInput")).SendKeys(_action.To.CoordinateX.ToString());
                _driver.FindElement(By.Id("yCoordInput")).SendKeys(_action.To.CoordinateY.ToString());

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

                _driver.FindElement(By.Id("btn_ok")).Click();
            }

            foreach(var w in _driver.WindowHandles)
            {
                _driver.SwitchTo().Window(w);
                _driver.FindElement(By.Id("btn_ok")).Click();
                result.Villages.Add(_action.To);
            }

            return result;
        }

        private void OpenNewTab(string url)
        {
            var windowHandles = _driver.WindowHandles;
            ((IJavaScriptExecutor)_driver).ExecuteScript(string.Format("window.open('{0}', '_blank');", url));
            var newWindowHandles = _driver.WindowHandles;
            var openedWindowHandle = newWindowHandles.Except(windowHandles).Single();
            _driver.SwitchTo().Window(openedWindowHandle);
            Task.Delay(3000);
        }

        private IWebElement WaitUntilElementExists(By elementLocator, int timeout = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(elementLocator));
                return element;
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Element with locator: '" + elementLocator + "' was not found in current context page.");
                throw;
            }
        }
    }
}
