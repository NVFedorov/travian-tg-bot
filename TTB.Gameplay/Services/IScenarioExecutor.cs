using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Scenarios;

namespace TTB.Gameplay.Services
{
    public interface IScenarioExecutor
    {
        BaseScenarioResult ExecuteScenario(IScenario scenario, ScenarioContext context);
    }
}
