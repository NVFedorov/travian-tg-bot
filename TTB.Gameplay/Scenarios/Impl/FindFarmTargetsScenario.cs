using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class FindFarmTargetsScenario : BaseScenario
    {
        private const int Targets = 100;
        private const int MinPopulation = 10;
        private const int MaxPopulation = 50;
        private const int MaxVillages = 1;

        public FindFarmTargetsScenario(IWebDriverProvider driverProvider) : base(driverProvider)
        {
        }

        protected override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            // open statistics
            _driver.FindElement(By.Id("n4")).Click();
            // go to last page
            _driver.FindElement(By.CssSelector("a.last")).Click();
            var result = new BaseScenarioResult();

            var maxPopulation = int.MaxValue;
            var targets = new List<string>();
            while (targets.Count < Targets || maxPopulation > MaxPopulation)
            {
                var rows = GetRows(_driver.FindElement(By.CssSelector("table#player"))).OrderByDescending(x => x.rank)
                    .Where(x => x.population >= MinPopulation &&
                        x.population <= MaxPopulation &&
                        string.IsNullOrWhiteSpace(x.alliance) &&
                        x.villages <= MaxVillages);

                if (rows.Any())
                {
                    maxPopulation = rows.Max(x => x.population);
                    targets.AddRange(rows.Select(x => x.url));
                }

                _driver.FindElement(By.CssSelector("a.previous")).Click();
            }

            foreach (var target in targets)
            {
                _driver.Navigate().GoToUrl(target);

                var playerName = _driver.FindElement(By.CssSelector("#content h1.titleInHeader")).Text;
                var villagesList = _driver.FindElements(By.CssSelector("#villages tbody tr"));
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
                        PlayerName = playerName,
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

                    result.Villages.Add(village);
                }
            }

            return result;
        }

        private List<(int rank, string url, string alliance, int population, int villages)> GetRows(IWebElement table)
        {
            var result = new List<(int rank, string url, string alliance, int population, int villages)>();
            var trs = table.FindElements(By.CssSelector("tbody>tr"));
            foreach (var tr in trs)
            {
                var success = int.TryParse(tr.FindElement(By.ClassName("pop")).Text, out var population);
                success &= int.TryParse(tr.FindElement(By.ClassName("ra")).Text.Replace(".", string.Empty), out var rank);
                success &= int.TryParse(tr.FindElement(By.ClassName("vil")).Text.Replace(".", string.Empty), out var villages);
                var url = tr.FindElement(By.CssSelector(".pla > a")).GetAttribute("href");
                var alliance = tr.FindElement(By.ClassName("al")).Text;
                if (success)
                {
                    result.Add((rank, url, alliance == "-" ? string.Empty : alliance, population, villages));
                }
            }

            return result;
        }
    }
}
