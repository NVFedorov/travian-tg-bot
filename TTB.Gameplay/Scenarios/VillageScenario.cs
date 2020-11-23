using OpenQA.Selenium;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios
{
    public abstract class VillageScenario<T> : BaseScenario where T : GameAction
    {
        protected readonly VillageDetailsModel _villageDetails;
        protected readonly T _action;

        public VillageScenario(IWebDriverProvider driverProvider, T action) : base(driverProvider)
        {
            this._action = action;
        }

        protected sealed override BaseScenarioResult ExecuteScenario(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            if (_action == null)
            {
                result.Errors.Add(new ScenarioError { ErrorMessage = "No actions found in scenario context.", ErrorSource = nameof(VillageScenario<T>) });
                return result;
            }

            var found = false;
            var villagesList = _driver.FindElements(By.CssSelector("#sidebarBoxVillagelist .content ul li a"));
            foreach (var v in villagesList)
            {
                var name = v.FindElement(By.CssSelector("span.name")).Text;
                if (this._action.Village.Name == name)
                {
                    found = true;
                    v.Click();
                    break;
                }
            }

            if (!found)
            {
                result.Errors.Add(new ScenarioError { ErrorMessage = $"Unable to find village {this._action.Village.Name}.", ErrorSource = nameof(VillageScenario<T>) });
                return result;
            }

            return this.ExecuteVillageActions(context);
        }

        protected abstract BaseScenarioResult ExecuteVillageActions(ScenarioContext context);
    }
}
