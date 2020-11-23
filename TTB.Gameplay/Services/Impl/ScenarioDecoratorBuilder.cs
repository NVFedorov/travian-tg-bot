using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Scenarios;

namespace TTB.Gameplay.Services.Impl
{
    public class ScenarioDecoratorBuilder
    {
        private readonly IScenarioFactory scenarioFactory;
        private readonly Stack<IScenario> scenarios;

        public ScenarioDecoratorBuilder(IScenarioFactory scenarioFactory)
        {
            this.scenarioFactory = scenarioFactory;
            this.scenarios = new Stack<IScenario>();
        }

        public ScenarioDecoratorBuilder WithScenario<T>() where T : BaseScenario
        {
            var scenario = this.scenarioFactory.GetScenario<T>() as IScenario;
            if (scenario == null)
            {
                throw new Exception($"The type must be derived from {nameof(BaseScenario)}");
            }

            this.scenarios.Push(scenario);
            return this;
        }

        public ScenarioDecoratorBuilder WithScenario(Type t)
        {
            var scenario = this.scenarioFactory.GetScenario(t) as IScenario;
            if (scenario == null)
            {
                throw new Exception($"The type must be derived from {nameof(BaseScenario)}");
            }

            this.scenarios.Push(scenario);
            return this;
        }

        public ScenarioDecoratorBuilder WithActionScenario<T, A>(A action) where T : VillageScenario<A> where A : GameAction
        {
            var scenario = this.scenarioFactory.GetScenario<T, A>(action) as VillageScenario<A>;
            if (scenario == null)
            {
                throw new Exception($"The type must be derived from {typeof(VillageScenario<A>)}");
            }

            this.scenarios.Push(scenario);
            return this;
        }

        public ScenarioDecoratorBuilder WithScenario(GameAction action)
        {
            var scenario = this.scenarioFactory.GetScenario(action) as IScenario;
            if (scenario == null)
            {
                throw new Exception($"The type must be derived from {typeof(VillageScenario<GameAction>)}");
            }

            this.scenarios.Push(scenario);
            return this;
        }

        public IScenario Build()
        {
            if (!this.scenarios.Any())
                return null;

            IScenario result = scenarios.Pop();
            IScenario current;
            while (this.scenarios.Any())
            {
                current = this.scenarios.Pop();
                current.Decorate(result);
                result = current;
            }

            return result;
        }
    }
}
