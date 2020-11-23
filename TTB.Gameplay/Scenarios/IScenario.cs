using System;
using System.Collections.Generic;
using System.Text;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;

namespace TTB.Gameplay.Scenarios
{
    public interface IScenario
    {
        BaseScenarioResult Execute(ScenarioContext context);
        void Decorate(IScenario scenario);
    }
}
