using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using TTB.Gameplay.Extensions;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class BuildScenario : VillageScenario<BuildAction>
    {
        public BuildScenario(IWebDriverProvider driverProvider, BuildAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            var result = new BaseScenarioResult();

            _driver.FindElement(By.CssSelector("#navigation > .village.resourceView")).Click();
            var field = _driver.FindElements(By.XPath($"//div[@id='resourceFieldContainer']/div[contains(@class, '{_action.BuildingId}') and contains(@class, '{_action.BuildingSlot}')]"))?.FirstOrDefault();
            
            if (field == null)
            {
                _driver.FindElement(By.CssSelector("#navigation > .village.buildingView")).Click();
                field = _driver.FindElements(By.XPath($"//div[@id='village_map']/div[contains(@class, '{_action.BuildingId}') and contains(@class, '{_action.BuildingSlot}')]/div"))?.FirstOrDefault();
                if (field == null)
                {
                    if (TryBuildNewBuilding(ref result))
                    {
                        return result;
                    }
                    else
                    {
                        result.Errors.Add(new ScenarioError
                        {
                            ErrorMessage = $"Unable to build new building.Building Id: {_action.BuildingId}, building slot: {_action.BuildingSlot}",
                            ErrorType = "NEW_BUILDING_FAILED"
                        });

                        return result;
                    }
                }
            }

            var css = field.GetAttribute("class");
            if (css.Contains("good", StringComparison.OrdinalIgnoreCase))
            {
                field.Click();
                var btn = _driver.FindElements(By.CssSelector("div.upgradeButtonsContainer .section1 button.green"));
                if (btn.Any())
                {
                    btn.First().Click();
                }
            }
            else if (css.Contains("notNow", StringComparison.OrdinalIgnoreCase))
            {
                field.Click();

                var contract = _driver.FindElements(By.Id("contract"))?.FirstOrDefault();
                if (contract == null)
                {
                    result.Errors.Add(new ScenarioError
                    {
                        ErrorMessage = $"Unable build new building with id: {_action.BuildingId}, slot: {_action.BuildingSlot}. Unable to locate contract.",
                        ErrorSource = nameof(BuildScenario)
                    });
                    return result;
                }

                var blocked = _driver.FindElements(By.CssSelector("div.upgradeBlocked"))?.FirstOrDefault();
                var blockCause = BuildErrorType.NoSpaceInQueue;
                if (blocked != null)
                {
                    if (blocked?.FindElements(By.CssSelector("div.errorMessage"))?.Any() ?? false)
                    {
                        blockCause = BuildErrorType.NotEnoughResources;
                    }
                }
                else
                {
                    field.Click();
                    var btn = _driver.FindElements(By.CssSelector("div.upgradeButtonsContainer.section2Enabled .section1 button.green"));
                    if (btn.Any())
                    {
                        btn.First().Click();
                        return result;
                    }
                }

                var error = new BuildScenarioError
                {
                    BuildErrorType = blockCause,
                    Village = _action.Village
                };

                if (blockCause == BuildErrorType.NotEnoughResources)
                {
                    var resources = GetResourcesOnWarehouse(_driver);
                    var contracts = _driver.FindElements(By.CssSelector("#contract div.resource span.value")).Select(x => int.Parse(x.Text.RemoveNonAsciiCharacters())).ToList();
                    if (contracts.Count != 5)
                    {
                        result.Errors.Add(new ScenarioError
                        {
                            ErrorMessage = $"Unable to find resources from contract for building building id: {_action.BuildingId}, slot: {_action.BuildingSlot}",
                            ErrorSource = nameof(BuildScenario)
                        });
                    }
                    var diff = new Resources
                    {
                        Lumber = resources.Lumber - contracts[0],
                        Clay = resources.Clay - contracts[1],
                        Iron = resources.Iron - contracts[2],
                        Crop = resources.Crop - contracts[3]
                    };

                    error.ResourcesNeeded = diff;
                }

                result.Errors.Add(error);

                //if (errType != BuildErrorType.NotEnoughCropProduction)
                //{
                //    _driver.FindElement(By.Id("closeContentButton")).Click();

                //}
            }
            else
            {
                var errType = css.Contains("underConstruction", StringComparison.OrdinalIgnoreCase)
                        ? BuildErrorType.BuildingIsUnderConstruction
                        : BuildErrorType.NotEnoughCropProduction;

                result.Errors.Add(new BuildScenarioError
                {
                    BuildErrorType = errType,
                    Village = _action.Village
                });

            }

            var update = GetVillageInfoScenario.UpdateInfo(_driver, _action.Village);
            result.Villages.Add(update);
            return result;
        }

        private bool TryBuildNewBuilding(ref BaseScenarioResult result)
        {
            var slotQuery = string.IsNullOrWhiteSpace(_action.BuildingSlot) ? string.Empty : $" and contains(@class, '{_action.BuildingSlot}')";
            var wrapper = _driver.FindElements(By.XPath($"//div[@id='village_map']/div[contains(@class, 'g0') {slotQuery}]"))?.FirstOrDefault();
            IWebElement contract = null;
            if (wrapper == null)
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = $"Unable to find building building id: {_action.BuildingId}, slot: {_action.BuildingSlot}",
                    ErrorSource = nameof(BuildScenario)
                });
                return false;
            }

            var field = wrapper.FindElements(By.CssSelector("svg g.clickShape path"))?.FirstOrDefault();
            if (field == null)
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = $"Unable to find building building id: {_action.BuildingId}, slot: {_action.BuildingSlot}",
                    ErrorSource = nameof(BuildScenario)
                });
                return false;
            }

            var script = field.GetAttribute("onclick");
            _driver.ExecuteJavaScript<object>(script);
            //field.Click();
            var id = _action.BuildingId.Replace("g", string.Empty);
                var container = WaitUntilElementExists(_driver, By.CssSelector("div.contentNavi.subNavi.tabWrapper .scrollingContainer"), 5);

            var nav = container.FindElements(By.CssSelector("div.content a"));

            if (nav == null || nav.Count != 3)
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = $"Unable build new building with id: {_action.BuildingId}, slot: {_action.BuildingSlot}. Unable to locate navigation on new buildings page.",
                    ErrorSource = nameof(BuildScenario)
                });
                return false;
            }

            var links = nav.Select(x => x.GetAttribute("href")).ToList();
            for (int i = 0; i < links.Count && contract == null; i++)
            {
                _driver.Navigate().GoToUrl(links[i]);
                contract = _driver.FindElements(By.Id($"contract_building{id}"))?.FirstOrDefault();
            }

            if (contract == null)
            {
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = $"Unable build new building with id: {_action.BuildingId}, slot: {_action.BuildingSlot}. Unable to locate navigation on new buildings page.",
                    ErrorSource = nameof(BuildScenario)
                });
                return false;
            }

            var btn = contract.FindElements(By.CssSelector("div.contractLink button.green"))?.FirstOrDefault();
            if (btn == null)
            {
                var errors = contract.FindElements(By.CssSelector("span.buildingCondition.error"));
                var errorsResult = new List<ScenarioError>();
                foreach (var error in errors)
                {
                    var condition = $"{error.FindElement(By.TagName("a")).Text} {error.FindElement(By.TagName("span")).Text}";
                    errorsResult.Add(new ScenarioError
                    {
                        ErrorMessage = $"Unable build new building with id: {_action.BuildingId}, slot: {_action.BuildingSlot}. Conditions not met: {condition}",
                        ErrorSource = nameof(BuildScenario)
                    });
                }

                result.Errors.AddRange(errorsResult);
                return false;
            }

            btn.Click();
            return true;
        }
    }
}
