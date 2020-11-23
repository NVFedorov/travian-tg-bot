using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class UpdateUserInfoScenario : BaseScenario
    {
        public UpdateUserInfoScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/spieler.php"));
            var villagesList = _driver.FindElements(By.CssSelector("#villages tbody tr"));

            var details = new List<Village>();
            foreach (var v in villagesList)
            {
                var name = v.FindElement(By.CssSelector(".name a")).Text;
                var capital = v.FindElements(By.CssSelector(".name span"));
                var xText = v.FindElement(By.ClassName("coordinateX")).Text;
                var yText = v.FindElement(By.ClassName("coordinateY")).Text;
                var successX = int.TryParse(Decode(xText), out int x);
                var successY = int.TryParse(Decode(yText), out int y);

                var village = new Village
                {
                    Name = name,
                    PlayerName = context.Player.UserName,
                    IsCapital = capital.Count > 0
                };

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

                details.Add(village);
            }

            result.Villages = details;


            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/dorf2.php"));
            var player = context.Player;
            var buildings = this._driver.FindElements(By.CssSelector(".buildingSlot"));
            if (buildings != null && buildings.Any())
            {
                var building = buildings[0];
                var classes = building.GetAttribute("class").Split(" ");
                var folk = classes[classes.Length - 1];
                folk = folk.ToUpperInvariant();
                if (Enum.TryParse(folk, out Tribe tribe))
                {
                    player.Tribe = tribe;
                }
                else
                {
                    result.Errors.Add(new ScenarioError
                    {
                        ErrorMessage = $"Unable to parse the Tribe: {folk}",
                        ErrorSource = nameof(UpdateUserInfoScenario)
                    });
                }
            }
            else
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = "Unable to find any building to update the Tribe",
                    ErrorSource = nameof(UpdateUserInfoScenario)
                });
            }

            this._driver.Navigate().GoToUrl(new Uri(context.Player.Uri, "/options.php"));
            var timeZoneId = this._driver.FindElement(By.XPath("//select[@name='timezone']//option[@selected='selected']"))?.Text;
            if (string.IsNullOrEmpty(timeZoneId))
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = "Unable to find time zone id",
                    ErrorSource = nameof(UpdateUserInfoScenario)
                });
            }
            else
            {
                player.TimeZone = timeZoneId;
            }

            var allianceElems = _driver.FindElements(By.XPath("//a[contains(@href, 'allianz.php')]"));
            if (allianceElems.Any())
            {
                player.Alliance = allianceElems.First().Text;
            }

            result.Player = player;
            return result;
        }
    }
}
