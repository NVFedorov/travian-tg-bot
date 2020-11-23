using System;
using System.Collections.Generic;
using System.Text;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Scenarios;

namespace TTB.Gameplay.Services
{
    public interface IScenarioFactory
    {
        IScenario GetScenario<T>() where T : BaseScenario;
        IScenario GetScenario(Type t);
        IScenario GetScenario<S, A>(A action) where S : VillageScenario<A> where A : GameAction;
        IScenario GetScenario(GameAction villageAction);
    }
}
