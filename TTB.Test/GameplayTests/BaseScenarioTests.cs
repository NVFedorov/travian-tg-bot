using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TTB.Gameplay.API;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Scenarios;
using TTB.Gameplay.Scenarios.Impl;
using TTB.Gameplay.Services;
using TTB.Gameplay.Services.Impl;

namespace TTB.Test.GameplayTests
{
    [Ignore("Selenium test")]
    [TestFixture]
    public class BaseScenarioTests
    {


        [Test]
        public async Task TestBaseScenario()
        {
            // test data should be loaded from a local file
            var player = new Player
            {
            };

            var login = new Mock<IScenario>();
            login.Setup(x => x.Execute(It.IsAny<ScenarioContext>()))
                .Returns(new BaseScenarioResult());
            var watch = new Mock<IScenario>();
            watch.Setup(x => x.Execute(It.IsAny<ScenarioContext>()))
                .Returns(new BaseScenarioResult());
            var check = new Mock<IScenario>();
            check.Setup(x => x.Execute(It.IsAny<ScenarioContext>()))
                .Returns(new BaseScenarioResult());

            var provider = new Mock<IWebDriverProvider>();
            Mock<IScenarioFactory> factory = new Mock<IScenarioFactory>();
            var logger = new Mock<ILogger<GameplayApi>>();
            factory.Setup(x => x.GetScenario<WatchScenario>()).Returns(watch.Object);
            factory.Setup(x => x.GetScenario<CheckReportsScenario>()).Returns(check.Object);
            var executor = new ScenarioExecutor();
            var api = new GameplayApi(executor, factory.Object, logger.Object);
            await api.Watch(player);

            login.Verify(x => x.Execute(It.IsAny<ScenarioContext>()), Times.Once);
        }
    }
}
