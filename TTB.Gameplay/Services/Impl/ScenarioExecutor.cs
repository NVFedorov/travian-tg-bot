using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Scenarios;

namespace TTB.Gameplay.Services.Impl
{
    public class ScenarioExecutor : IScenarioExecutor
    {
        private static object locker = new object();

        public BaseScenarioResult ExecuteScenario(IScenario scenario, ScenarioContext context)
        {
            lock (locker)
            {
                return scenario.Execute(context);
            }
        }
    }
}
