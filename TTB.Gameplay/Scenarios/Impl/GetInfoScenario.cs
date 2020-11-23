using System;
using System.Collections.Generic;
using System.Text;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Services;

namespace TTB.Gameplay.Scenarios.Impl
{
    public class GetInfoScenario : VillageScenario<GameAction>
    {
        public GetInfoScenario(IWebDriverProvider driverProvider, GameAction action) : base(driverProvider, action)
        {
        }

        protected override BaseScenarioResult ExecuteVillageActions(ScenarioContext context)
        {
            var result = new BaseScenarioResult();
            var village = GetVillageInfoScenario.UpdateInfo(_driver, _action.Village);
            village = CommonScenario.UpdateResources(_driver, context, _action.Village);
            result.Villages.Add(village);
            return result;
        }
    }
}
