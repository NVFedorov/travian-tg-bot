using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using TTB.Gameplay.Extensions;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios
{
    public abstract class CommonScenario : BaseScenario
    {
        public CommonScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected sealed override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            var villagesList = this._driver.FindElements(By.CssSelector("#sidebarBoxVillagelist .content ul li a"));

            Dictionary<string, Village> villagesLinks = new Dictionary<string, Village>();
            foreach (var v in villagesList)
            {
                var name = v.FindElement(By.CssSelector("span.name")).Text;
                var village = new Village
                {
                    Name = name,
                    PlayerName = context.Player.UserName                    
                };
                                
                var xText = v.FindElement(By.ClassName("coordinateX")).Text;
                var yText = v.FindElement(By.ClassName("coordinateY")).Text;
                var successX = int.TryParse(Decode(xText), out int x);
                var successY = int.TryParse(Decode(yText), out int y);
                if (successX && successY)
                {
                    village.CoordinateX = x;
                    village.CoordinateY = y;
                }
                else
                {
                    result.Errors.Add(new ScenarioError
                    {
                        ErrorSource = nameof(CommonScenario),
                        ErrorMessage = $"Unable to parse coordinates for village {name}. Input values: x = [{xText}], y = [{yText}]",
                        ErrorType = "UpdateInfo"
                    });
                }

                villagesLinks.Add(v.GetAttribute("href"), village);
            }

            foreach(var link in villagesLinks)
            {
                this._driver.Navigate().GoToUrl(link.Key);
                var villageResult = this.ExecuteInVillage(context, link.Value);
                villageResult = UpdateResources(_driver, context, villageResult);
                result.Villages.Add(villageResult);
            }

            result.IsUserUnderAttack = result.Villages.Any(x => x.IsUnderAttack || x.Attacks.Any());
            return result;
        }

        protected abstract Village ExecuteInVillage(ScenarioContext context, Village source);

        public static Village UpdateResources(IWebDriver driver, ScenarioContext context, Village village)
        {
            driver.FindElement(By.CssSelector("#navigation > .village.resourceView")).Click();
            var warehouse = driver.FindElement(By.CssSelector("#stockBar .warehouse .capacity .value")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            village.Warehourse = int.Parse(warehouse);
            var granary = driver.FindElement(By.CssSelector("#stockBar .granary .capacity .value")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            village.Granary = int.Parse(granary);

            village.Resources = GetResourcesOnWarehouse(driver);

            var productionaTable = driver.FindElements(By.CssSelector("#production tbody tr td.num"));
            if (productionaTable.Count < 4)
            {
                return village;
            }


            var lumberProduction = productionaTable[0].Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var clayProduction = productionaTable[1].Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var ironProduction = productionaTable[2].Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();

            //driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/production.php?t=1"));
            //var lumberProduction = driver.FindElement(By.CssSelector("#content .productionPerHourTotal .total .numberCell")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            //driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/production.php?t=2"));
            //var clayProduction = driver.FindElement(By.CssSelector("#content .productionPerHourTotal .total .numberCell")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            //driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/production.php?t=3"));
            //var ironProduction = driver.FindElement(By.CssSelector("#content .productionPerHourTotal .total .numberCell")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/production.php?t=5"));
            var cropNumberCell = WaitUntilElementExists(driver, By.CssSelector(".balanceCropBalancePart .subtotal .numberCell"));
            var cropProduction = cropNumberCell.Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();
            var freeCrop = driver.FindElement(By.CssSelector(".balanceCropBalancePart .subtotal .numberCell")).Text.Replace(" ", string.Empty).RemoveNonAsciiCharacters();

            village.ResourcesProduction = new Resources
            {
                Lumber = int.Parse(lumberProduction),
                Clay = int.Parse(clayProduction),
                Iron = int.Parse(ironProduction),
                Crop = int.Parse(cropProduction),
                FreeCrop = int.Parse(freeCrop)
            };

            return village;
        }
    }
}
