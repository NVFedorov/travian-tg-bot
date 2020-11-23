using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Scenarios.Impl;
using TTB.Gameplay.Services;
using TTB.Gameplay.Services.Impl;

namespace TTB.Test.ServicesTests
{
    [TestFixture]
    public class ScenarioFactoryTests
    {
        [Test]
        public void TestScenarioCreationFromAction()
        {
            var provider = new Mock<IWebDriverProvider>();
            var factory = new ScenarioFactory(provider.Object);
            var action = new TrainArmyAction
            {
                UnitsToTrain = new Dictionary<string, int> { { "test", 1 } }
            };

            var scenario = factory.GetScenario(action);
            Assert.NotNull(scenario);
            Assert.AreEqual(typeof(TrainArmyScenario), scenario.GetType());

            var buildAction = new BuildAction
            {
                BuildingId = "testBuilding"
            };

            var buildScenario = factory.GetScenario(buildAction);
            Assert.NotNull(buildScenario);
            Assert.AreEqual(typeof(BuildScenario), buildScenario.GetType());
        }

        [Test]
        public void Test_PrepareToAttack_Scenarios_Creation()
        {
            var actions = new List<GameAction>
            {
                new SendResourcesAction(),
                new TrainArmyAction(),
                new SendArmyAction()
            };

            var provider = new Mock<IWebDriverProvider>();
            var factory = new ScenarioFactory(provider.Object);
            var scenarioBuilder = new ScenarioDecoratorBuilder(factory);
            foreach (var action in actions)
            {
                scenarioBuilder = scenarioBuilder.WithScenario(action);
            }

            var scenario = scenarioBuilder.Build();

            Assert.IsNotNull(scenario);
            Assert.AreEqual(typeof(SendResourcesScenario), scenario.GetType());
        }
    }
}
