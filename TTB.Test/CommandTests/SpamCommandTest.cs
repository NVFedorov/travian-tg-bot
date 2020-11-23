using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Services;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Test.Fake;

namespace TTB.Test.CommandTests
{
    [TestFixture]
    public class SpamCommandTest
    {
        private const long chatId = 123;

        private FakeServiceProviderBuilder _serviceBuilder;
        private Mock<IGameplayClient> _clientMock;
        private Mock<IVillageRepository> _villageRepoMock;
        private Mock<IServiceProvider> _serviceMock;
        private Mock<ITravianUserRepository> _travianUserRepoMock;
        private Mock<ILogger<AbstractCommand>> _logger;
        private SpamCommand cmd;


        [SetUp]
        public void SetUp()
        {
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

            var user = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET);
            _travianUserRepoMock = new Mock<ITravianUserRepository>();
            _travianUserRepoMock.Setup(x => x.GetActiveUser(FakeDataProvider.BotUserName))
                .Returns(Task.FromResult(user));

            _logger = new Mock<ILogger<AbstractCommand>>();
            var botService = new Mock<IBotService>();
            botService.Setup(x => x.SendTextMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Message()));

            var unitProvider = new UnitsProviderFake();

            _villageRepoMock = new Mock<IVillageRepository>();
            _serviceBuilder = FakeServiceProviderBuilder.Builder()
                .WithService(mapper)
                .WithService(botService.Object)
                .WithService(mgr.Object)
                .WithService(_logger.Object)
                .WithService(unitProvider.GetUnitRepository())
                .WithService(_travianUserRepoMock.Object);
        }

        [Test]
        public void TestSendArmyToCorrectVillage()
        {
            var result = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = Builder<Village>.CreateListOfSize(2)
                    .All()
                    .With(y => y.Alliance = "TestAlliance")
                    .TheFirst(1)
                    .With(y => y.CoordinateX = 10)
                    .With(y => y.CoordinateY = 10)
                    .TheNext(1)
                    .With(y => y.CoordinateX = -10)
                    .With(y => y.CoordinateY = -10)
                    .Build().ToList())
                .Build();

            _clientMock = new Mock<IGameplayClient>();
            _clientMock.Setup(x => x.GetTargetsFromMessage(It.IsAny<TravianUser>(), It.IsAny<string>()))
                .Returns(Task.FromResult(result));

            var calledActions = new List<SendArmyAction>();
            _clientMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<SendArmyAction>>()))
                .Callback<TravianUser, IEnumerable<SendArmyAction>>((tuser, actions) =>
                {
                    calledActions.AddRange(actions);
                })
                .Returns(Task.FromResult(new BaseScenarioResult { Success = true }));

            _villageRepoMock.Setup(x => x.GetVillages(FakeDataProvider.TravianUserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)
                    Builder<VillageModel>.CreateListOfSize(2)
                    .TheFirst(1)
                    .With(x => x.CoordinateX = 11)
                    .With(x => x.CoordinateY = 12)
                    .With(x => x.IsSpamFeatureOn = true)
                    .With(x => x.SpamUnits = new Dictionary<string, int>
                        {
                            { "phalang", 19 },
                            { "swordsman", 1 }
                        })
                    .TheNext(1)
                    .With(x => x.IsSpamFeatureOn = false)
                    .Build()));

            _serviceBuilder
                .WithService(_clientMock.Object)
                .WithService(_villageRepoMock.Object);

            cmd = new SpamCommand(_serviceBuilder.Build(), chatId);
            Assert.DoesNotThrowAsync(async() => await cmd.Execute("separated string"));
            _logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            Assert.AreEqual(2, calledActions.Count);
            Assert.AreEqual(11, calledActions.First().Village.CoordinateX);
            Assert.AreEqual(12, calledActions.First().Village.CoordinateY);
            Assert.AreEqual(10, calledActions.First().To.CoordinateX);
            Assert.AreEqual(10, calledActions.First().To.CoordinateY);

            Assert.AreEqual(11, calledActions.Last().Village.CoordinateX);
            Assert.AreEqual(12, calledActions.Last().Village.CoordinateY);
            Assert.AreEqual(-10, calledActions.Last().To.CoordinateX);
            Assert.AreEqual(-10, calledActions.Last().To.CoordinateY);
            Assert.IsTrue(calledActions.Last().UnitsToSend.ContainsKey("Фаланга"));
            Assert.AreEqual(19, calledActions.Last().UnitsToSend["Фаланга"]);
            Assert.IsTrue(calledActions.Last().UnitsToSend.ContainsKey("Мечник"));
            Assert.AreEqual(1, calledActions.Last().UnitsToSend["Мечник"]);
        }
    }
}
