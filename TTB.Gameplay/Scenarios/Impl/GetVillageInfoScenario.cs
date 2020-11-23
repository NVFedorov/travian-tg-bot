using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class GetVillageInfoScenario : CommonScenario
    {
        public GetVillageInfoScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override Village ExecuteInVillage(ScenarioContext context, Village source)
        {
            return UpdateInfo(_driver, source);
        }

        public static Village UpdateInfo (IWebDriver driver, Village village)
        {
            driver.FindElement(By.CssSelector("#navigation > .village.resourceView")).Click();
            var container = WaitUntilElementExists(driver, By.Id("resourceFieldContainer"), 5);
            var fields = container.FindElements(By.XPath("//div[contains(@class, 'buildingSlot')]"));

            var result = new List<BuildingSlot>();
            foreach (var field in fields)
            {
                var css = field.GetAttribute("class");
                var splitted = css.Split(" ");
                var id = splitted.FirstOrDefault(x => x.Contains("gid"));
                var slot = splitted.FirstOrDefault(x => x.Contains("buildingSlot"));
                var match = Regex.Match(css, @"level\d\d?", RegexOptions.IgnoreCase);
                var stateMatch = Regex.Match(css, @"(^|\s)(good|notNow|maxLevel|underConstruction)\s", RegexOptions.IgnoreCase);
                if (!match.Success || !int.TryParse(match.Value.Replace("level", string.Empty), out int l))
                    l = -1;
                
                if (!stateMatch.Success || !Enum.TryParse<BuildingSlotState>(stateMatch.Value, true, out var state))
                    state = BuildingSlotState.Gray;

                result.Add(new BuildingSlot
                {
                    Id = slot,
                    BuildingId = id,
                    Level = l,
                    State = state
                });
            }

            var canBuildinDorf1 = driver.FindElements(By.XPath("//div[contains(@class, 'buildingSlot') and contains(@class, 'good')]"))?.Any() ?? false;

            driver.FindElement(By.CssSelector("#navigation > .village.buildingView")).Click();
            var slots = driver.FindElement(By.Id("village_map")).FindElements(By.XPath("//div[contains(@class, 'buildingSlot')]"));
            foreach (var slot in slots)
            {
                var css = slot.GetAttribute("class");
                var matchId = Regex.Match(css, @"g\d\d?", RegexOptions.IgnoreCase);
                var id = matchId.Success ? matchId.Value : string.Empty;

                var matchSlotId = Regex.Match(css, @"aid\d\d?", RegexOptions.IgnoreCase);
                var slotId = matchSlotId.Success ? matchSlotId.Value : string.Empty;
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(slotId))
                {
                    var level = slot.FindElements(By.CssSelector("div.labelLayer"))?.FirstOrDefault()?.Text;
                    if (string.IsNullOrEmpty(level) || !int.TryParse(level.Replace("level", string.Empty), out int l))
                        l = -1;

                    var colorLayerCss = slot.FindElements(By.CssSelector("div.level"))?.FirstOrDefault()?.GetAttribute("class");
                    var state = BuildingSlotState.Empty;
                    if (!string.IsNullOrEmpty(colorLayerCss))
                    {
                        var stateMatch = Regex.Match(colorLayerCss, @"(^|\s)(good|notNow|maxLevel|underConstruction)\s", RegexOptions.IgnoreCase);
                        if (!stateMatch.Success || !Enum.TryParse<BuildingSlotState>(stateMatch.Value, true, out state))
                            state = BuildingSlotState.Gray;
                    }                        

                    result.Add(new BuildingSlot
                    {
                        Id = slotId,
                        BuildingId = id,
                        Level = l,
                        State = state
                    });
                }
            }

            var canBuildInDorf2 = driver.FindElements(By.XPath("//div[contains(@class, 'buildingSlot')]/div[contains(@class, 'good')]"))?.Any() ?? false;

            // update buildings
            village.BuildingSlots = result;
            village.CanBuild = canBuildinDorf1 || canBuildInDorf2;

            var buildingList = driver.FindElements(By.CssSelector(".boxes.buildingList"))?.FirstOrDefault();
            var dorf1Time = TimeSpan.Zero;
            var dorf2Time = TimeSpan.Zero;
            if (buildingList != null)
            {
                var durations = buildingList.FindElements(By.CssSelector("div.buildDuration span.timer"));
                if (durations.Any())
                {
                    var timers = durations.Select(x => TimeSpan.FromSeconds(int.Parse(x.GetAttribute("value")))).OrderBy(x => x.TotalMilliseconds).ToList();
                    dorf1Time = timers[0];
                    if (timers.Count > 1)
                    {
                        dorf2Time = timers[1];
                    }
                }
            }

            village.Dorf1BuildTimeLeft = dorf1Time;
            village.Dorf2BuildTimeLeft = dorf2Time;

            return village;
        }
    }
}
