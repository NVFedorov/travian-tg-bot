using System;
using System.Collections.Generic;
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
    public class GameplayApiTests
    {
        [Test]
        public void TestRunMultipleScenariosAtOnce()
        {
            var user1 = new Player
            {
                UserName = "user1"
            };
            var user2 = new Player
            {
                UserName = "user2"
            };
            var user3 = new Player
            {
                UserName = "user3"
            };

            var results = new List<string>();
            Func<ScenarioContext, BaseScenarioResult> execute = inp =>
            {
                Task.Delay(1000 + new Random().Next(2000)).Wait();
                results.Add(inp.Player.UserName);
                return new BaseScenarioResult { Messages = new List<string> { inp.Player.UserName } };
            };

            var watch = new Mock<IScenario>();
            var updateUserInfo = new Mock<IScenario>();
            watch.Setup(x => x.Execute(It.IsAny<ScenarioContext>()))
                .Returns(execute);

            var check = new Mock<IScenario>();

            var provider = new Mock<IWebDriverProvider>();
            var factory = new Mock<IScenarioFactory>();
            factory.Setup(x => x.GetScenario<WatchScenario>()).Returns(watch.Object);
            factory.Setup(x => x.GetScenario<CheckReportsScenario>()).Returns(check.Object);
            factory.Setup(x => x.GetScenario<UpdateUserInfoScenario>()).Returns(updateUserInfo.Object);
            var executor = new ScenarioExecutor();
            var logger = new Mock<ILogger<GameplayApi>>();
            var api = new GameplayApi(executor, factory.Object, logger.Object);

            var tasks = Task.WhenAll(api.Watch(user1), api.Watch(user3), api.Watch(user2));
            tasks.Wait();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("user1", results[0]);
            Assert.AreEqual("user3", results[1]);
            Assert.AreEqual("user2", results[2]);
        }
    }
}
