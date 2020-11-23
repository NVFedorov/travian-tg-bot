using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using Telegram.Bot.Types;
using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Services;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Test.Fake;

namespace TTB.Test.JobTests
{
    [TestFixture]
    public class PrepareToAttackJobTests
    {
        private FakeServiceProviderBuilderForJob _serviceBuilder;

        [SetUp]
        public void Setup()
        {
            _serviceBuilder = FakeServiceProviderBuilderForJob.DefaultBuilder();
        }

        [Test]
        public void Test_Prepare_To_Attack()
        {
            var actionProviderMock = new Mock<IActionProvider>();
            actionProviderMock.Setup(x => x.GetActionsForPlayer(It.IsNotNull<TravianUser>()))
                .Returns(Task.FromResult((IEnumerable<GameAction>)Builder<GameAction>
                    .CreateListOfSize(2)
                    .TheFirst(1)
                    .With(x => x.Action = GameActionType.TRAIN_ARMY)
                    .With(x => x.Village = Builder<Village>.CreateNew().With(y => y.Name = "test_village_1").Build())
                    .TheNext(1)
                    .With(x => x.Action = GameActionType.SEND_RESOURCES)
                    .With(x => x.Village = Builder<Village>.CreateNew().With(y => y.Name = "test_village_2").Build())
                    .Build()));
            var gameplayMock = new Mock<IGameplayClient>();
            gameplayMock
                .Setup(x => x.ExecuteActions(It.IsNotNull<TravianUser>(), It.IsNotNull<List<GameAction>>()))
                .Returns(Task.FromResult(Builder<BaseScenarioResult>.CreateNew().Build()));

            _serviceBuilder = _serviceBuilder
                .WithService(actionProviderMock.Object)
                .WithService(gameplayMock.Object);

            var job = new PrepareToAttackJob(_serviceBuilder.Build());
            //var context = new Mock<IJobExecutionContext>();
            //var jobDetail = new Mock<IJobDetail>();
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK, true),
                JobType = typeof(PrepareToAttackJob)
            };
            //jobDetail.Setup(x => x.JobDataMap[AbstractJob.JobExecutionDataKey]).Returns(data);
            //context.Setup(x => x.JobDetail).Returns(jobDetail.Object);
            Assert.DoesNotThrowAsync(async () => await job.Execute(data));

            _serviceBuilder.LoggerMock.Verify(x => x.Log(
                LogLevel.Error, 
                It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), 
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            Assert.AreEqual(2, _serviceBuilder.Messages.Count);
            Assert.AreEqual($"The village test_village_1 was prepared to attack with following action: [TRAIN_ARMY].", _serviceBuilder.Messages[0]);
            Assert.AreEqual($"The village test_village_2 was prepared to attack with following action: [SEND_RESOURCES].", _serviceBuilder.Messages[1]);
        }
    }
}
