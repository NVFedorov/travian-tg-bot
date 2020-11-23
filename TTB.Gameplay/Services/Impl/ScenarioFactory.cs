using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Scenarios;
using TTB.Gameplay.Scenarios.Impl;

namespace TTB.Gameplay.Services.Impl
{
    public class ScenarioFactory : IScenarioFactory
    {
        private readonly IWebDriverProvider driverProvider;

        public ScenarioFactory(IWebDriverProvider driverProvider)
        {
            this.driverProvider = driverProvider;
        }

        public IScenario GetScenario<T>() where T : BaseScenario
        {
            return (T)Activator.CreateInstance(typeof(T), this.driverProvider);
        }

        public IScenario GetScenario(Type t)
        {
            if (!t.IsSubclassOf(typeof(BaseScenario)))
            {
                throw new Exception($"Type {t} must be derived from {typeof(BaseScenario)}");
            }

            return (IScenario)Activator.CreateInstance(t, this.driverProvider);
        }

        public IScenario GetScenario<S, A>(A action) where S : VillageScenario<A> where A : GameAction
        {
            return (IScenario)Activator.CreateInstance(typeof(S), this.driverProvider, action);
        }

        public IScenario GetScenario(GameAction villageAction)
        {
            var abstractType = typeof(VillageScenario<>).MakeGenericType(villageAction.GetType());
            var concreteType = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => !x.IsAbstract && x.IsSubclassOf(abstractType)).ToList().FirstOrDefault();

            if (concreteType == null)
            {
                throw new Exception($"Type {concreteType} must be derived from {typeof(VillageScenario<>)}");
            }

            return (IScenario)Activator.CreateInstance(concreteType, this.driverProvider, villageAction);
        }
    }
}
