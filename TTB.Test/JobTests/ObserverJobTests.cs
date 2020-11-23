using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Quartz;
using Telegram.Bot.Types;
using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Services;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.Results;
using TTB.Test.Fake;

namespace TTB.Test.JobTests
{
    [TestFixture]
    public class ObserverJobTests
    {
        private FakeServiceProviderBuilder _serviceBuilder;
        private List<string> _reportMsgs;
        private List<TravianUser> _updates;
        private Mock<ILogger<AbstractJob>> _logger;
        private Mock<ICommandFactory> _cmdFactoryMock;
        private Mock<ITravianUserRepository> _travianUserRepoMock;

        [SetUp]
        public void Setup()
        {
            _reportMsgs = new List<string>();
            var botService = new Mock<IBotService>();
            botService.Setup(x => x.SendTextMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Callback<long, string>((cid, msg) => _reportMsgs.Add(msg))
                .Returns(Task.FromResult(new Message()));
            var botUser = Builder<BotUser>.CreateNew().With(x => x.UserName = FakeDataProvider.BotUserName).Build();
            var mgr = new Mock<IBotUserProvider>();
            mgr.Setup(x => x.FindByNameAsync(FakeDataProvider.BotUserName))
                .Returns(Task.FromResult(botUser));
            mgr.Setup(x => x.FindByChatId(It.IsAny<long>()))
                .Returns(botUser);
            IMapper mapper = new Mapper(
                new MapperConfiguration(
                configure =>
                {
                    configure.AddProfile<MappingProfile>();
                })
            );

            _cmdFactoryMock = new Mock<ICommandFactory>();
            _cmdFactoryMock.Setup(x => x.GetQueueableCommand(nameof(PrepareToAttackCommand), It.IsAny<long>()))
                .Returns(new FakeCommand());
            _logger = new Mock<ILogger<AbstractJob>>();
            _travianUserRepoMock = new Mock<ITravianUserRepository>();
            _updates = new List<TravianUser>();
            _travianUserRepoMock.Setup(x => x.Update(It.IsAny<TravianUser>()))
                .Callback<TravianUser>(user =>
                {
                    _updates.Add(user);
                })
                .Returns(Task.FromResult(true));
            _travianUserRepoMock.Setup(x => x.ReplacePlayerData(It.IsAny<TravianUser>()))
                .Callback<TravianUser>(user =>
                {
                    _updates.Add(user);
                })
                .Returns(Task.FromResult(true));
            _travianUserRepoMock.Setup(x => x.ReplacePlayerDataVillages(It.IsAny<TravianUser>()))
                .Callback<TravianUser>(user =>
                {
                    _updates.Add(user);
                })
                .Returns(Task.FromResult(true));


            IGameplayClient gameplay = new Fake.GameplayClientFake(mapper);
            var unitRepo = new UnitsProviderFake().GetUnitRepository();
            _serviceBuilder = FakeServiceProviderBuilder.Builder()
                .WithService(unitRepo)
                .WithService(mapper)
                .WithService(gameplay)
                .WithService(botService.Object)
                .WithService(mgr.Object)
                .WithService(_logger.Object)
                .WithService(_cmdFactoryMock.Object)
                .WithService(_travianUserRepoMock.Object);
        }

        [Test]
        public void TestObserverJobExecution_When_It_Was_Quiet_And_Gameplay_Returns_Attack()
        {
            var villageRepoMock = new Mock<IVillageRepository>();
            villageRepoMock.Setup(x => x.GetVillages(FakeDataProvider.TravianUserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)FakeDataProvider.GetVillagesFromDatabase(false)));

            _serviceBuilder = _serviceBuilder
                .WithService(villageRepoMock.Object);

            var observerJob = new ObserverJob(_serviceBuilder.Build());
            var context = new Mock<IJobExecutionContext>();
            var jobDetail = new Mock<IJobDetail>();
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET, true)
            };
            jobDetail.Setup(x => x.JobDataMap[AbstractJob.JobExecutionDataKey]).Returns(data);
            context.Setup(x => x.JobDetail).Returns(jobDetail.Object);
            Assert.DoesNotThrowAsync(async () => await observerJob.Execute(data));
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _cmdFactoryMock.Verify(x => x.GetQueueableCommand(nameof(PrepareToAttackCommand), It.IsAny<long>()), Times.Exactly(2));
            _travianUserRepoMock.Verify(x => x.ReplacePlayerDataVillages(It.IsAny<TravianUser>()), Times.Once);

            Assert.AreEqual(7, _reportMsgs.Count);
            Assert.IsTrue(_reportMsgs[0].Contains($"Player {FakeDataProvider.TravianUserName} has been scanned"));
            Assert.IsTrue(_reportMsgs[1].Contains($"Player {FakeDataProvider.TravianUserName} has been scanned"));
            Assert.IsTrue(_reportMsgs[2].Contains($"Player {FakeDataProvider.TravianUserName} is under attack:"));
            Assert.IsTrue(_reportMsgs[2].Contains("The village [test_village_under_attack] is under attack"));
            Assert.IsTrue(_reportMsgs[3].Contains($"Player {FakeDataProvider.TravianUserName} is under attack:"));
            Assert.IsTrue(_reportMsgs[3].Contains("The village [test_village_under_attack] is under attack"));
            Assert.AreEqual(_reportMsgs[4], $"New incoming attacks discovered for player [{FakeDataProvider.TravianUserName}]");
            Assert.IsTrue(_reportMsgs[5].Contains($"Village [{FakeDataProvider.TestVillageName}] is under attack"));
            Assert.IsTrue(_reportMsgs[5].Contains($"The attack date time:"));
            Assert.IsTrue(_reportMsgs[5].Contains($"The intruder:"));
            Assert.IsTrue(_reportMsgs[6].Contains($"Village [{FakeDataProvider.TestVillageName}] is under attack"));
            Assert.IsTrue(_reportMsgs[6].Contains($"The attack date time:"));
            Assert.IsTrue(_reportMsgs[6].Contains($"The intruder:"));

            Assert.AreEqual(2, _updates.Count);
            Assert.AreEqual(PlayerStatus.UNDER_ATTACK, _updates.First().PlayerData.Status);
        }

        [Test]
        public void TestObserverJobExecution_When_User_Was_Under_Attack_Gameplay_Returns_Attack()
        {
            var villageRepoMock = new Mock<IVillageRepository>();
            villageRepoMock.Setup(x => x.GetVillages(FakeDataProvider.TravianUserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)FakeDataProvider.GetVillagesFromDatabase(true)));

            _serviceBuilder = _serviceBuilder
                .WithService(villageRepoMock.Object);

            var observerJob = new ObserverJob(_serviceBuilder.Build());
            var context = new Mock<IJobExecutionContext>();
            var jobDetail = new Mock<IJobDetail>();
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK, true)
            };
            jobDetail.Setup(x => x.JobDataMap[AbstractJob.JobExecutionDataKey]).Returns(data);
            context.Setup(x => x.JobDetail).Returns(jobDetail.Object);
            Assert.DoesNotThrowAsync(async () => await observerJob.Execute(data));
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _cmdFactoryMock.Verify(x => x.GetQueueableCommand(nameof(PrepareToAttackCommand), It.IsAny<long>()), Times.Once);
            _travianUserRepoMock.Verify(x => x.ReplacePlayerDataVillages(It.IsAny<TravianUser>()), Times.Once);

            Assert.AreEqual(2, _reportMsgs.Count);
            Assert.IsTrue(_reportMsgs[1].Contains($"Village [test_village_under_attack] is under attack."));
            Assert.IsTrue(_reportMsgs[1].Contains(FakeDataProvider.NewAttackDateTime.ToDisplayStringApplyTimeZone("UTC+2")));

            Assert.AreEqual(1, _updates.Count);
            Assert.AreEqual(PlayerStatus.UNDER_ATTACK, _updates.First().PlayerData.Status);
        }

        [Test]
        public void TestObserverJobExecution_When_User_Was_Under_Attack_Gameplay_Returns_All_Quiet()
        {
            var villageRepoMock = new Mock<IVillageRepository>();
            villageRepoMock.Setup(x => x.GetVillages(FakeDataProvider.TravianUserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)FakeDataProvider.GetVillagesFromDatabase(false)));

            var user = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK);
            var result = Builder<BaseScenarioResult>
                .CreateNew()
                .With(x => x.Villages = Builder<Village>.CreateListOfSize(2).All().With(y => y.Attacks = null).Build().ToList())
                .Build();

            var gameplayMock = new Mock<IGameplayClient>();
            gameplayMock.Setup(x => x.RunScan(It.IsAny<TravianUser>(), It.IsAny<bool>())).Returns(Task.FromResult(result));

            _serviceBuilder = _serviceBuilder
                .WithService(villageRepoMock.Object)
                .WithService(gameplayMock.Object);

            var observerJob = new ObserverJob(_serviceBuilder.Build());
            var context = new Mock<IJobExecutionContext>();
            var jobDetail = new Mock<IJobDetail>();
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK, true)
            };
            jobDetail.Setup(x => x.JobDataMap[AbstractJob.JobExecutionDataKey]).Returns(data);
            context.Setup(x => x.JobDetail).Returns(jobDetail.Object);
            Assert.DoesNotThrowAsync(async () => await observerJob.Execute(data));
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            _travianUserRepoMock.Verify(x => x.ReplacePlayerDataVillages(It.IsAny<TravianUser>()), Times.Once);
            _travianUserRepoMock.Verify(x => x.ReplacePlayerData(It.IsAny<TravianUser>()), Times.Once);
            _travianUserRepoMock.Verify(x => x.ReplacePlayerData(It.IsAny<TravianUser>()), Times.Once);

            Assert.AreEqual(2, _updates.Count);
            Assert.AreEqual(PlayerStatus.ALL_QUIET, _updates.First().PlayerData.Status);
        }
    }
}
