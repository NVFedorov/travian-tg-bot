using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Services;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;

namespace TTB.Test.Fake
{
    public class FakeServiceProviderBuilderForJob : FakeServiceProviderBuilder
    {

        private FakeServiceProviderBuilderForJob() : base()
        {
            Messages = new List<string>();
            LoggerMock = new Mock<ILogger<AbstractJob>>();
        }

        public List<string> Messages { get; private set; }
        public Mock<ILogger<AbstractJob>> LoggerMock { get; private set; }
        public BotUser BotUser { get; private set; }
        public TravianUser TravianUser { get; private set; }
        public IMapper Mapper { get; private set; }
        public IUnitRepository UnitRepository { get; private set; }

        public new static FakeServiceProviderBuilderForJob Builder()
        {
            var builder = new FakeServiceProviderBuilderForJob();
            var botService = new Mock<IBotService>();
            botService.Setup(x => x.SendTextMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Callback<long, string>((cid, msg) => builder.Messages.Add(msg))
                .Returns(Task.FromResult(new Message()));

            builder = builder
                .WithService(botService.Object)
                .WithService(builder.LoggerMock.Object);

            return builder;
        }

        public static FakeServiceProviderBuilderForJob DefaultBuilder()
        {
            var builder = Builder()
                .AddMapper()
                .AddUser()
                .AddTravianUser();

            return builder;
        }

        public new FakeServiceProviderBuilderForJob WithService<T>(T service)
        {
            _serviceMock.Setup(x => x.GetService(typeof(T))).Returns(service);
            return this;
        }

        public FakeServiceProviderBuilderForJob AddMapper()
        {
            Mapper = new Mapper(
                new MapperConfiguration(
                configure =>
                {
                    configure.AddProfile<MappingProfile>();
                })
            );
            
            return this.WithService(Mapper);
        }

        public FakeServiceProviderBuilderForJob AddUser(BotUser user = null)
        {
            if (user == null)
            {
                user = Builder<BotUser>.CreateNew().With(x => x.UserName = FakeDataProvider.BotUserName).Build();
            }

            BotUser = user;

            var mgr = new Mock<IBotUserProvider>();
            mgr.Setup(x => x.FindByNameAsync(user.UserName))
                .Returns(Task.FromResult(user));
            mgr.Setup(x => x.FindByChatId(It.IsAny<long>()))
                .Returns(user);

            return this.WithService(mgr.Object);
        }

        public FakeServiceProviderBuilderForJob AddTravianUser(TravianUser travianUser = null)
        {
            if (BotUser == null)
                throw new InvalidOperationException("Can not add travian user before bot user. Define the bot user first.");

            if (travianUser == null)
                travianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET);
            TravianUser = travianUser;

            var travianUserRepoMock = new Mock<ITravianUserRepository>();
            travianUserRepoMock.Setup(x => x.GetActiveUser(BotUser.UserName))
                .Returns(Task.FromResult(travianUser));

            return this.WithService(travianUserRepoMock.Object);
        }

        public FakeServiceProviderBuilderForJob AddUnitRepository()
        {
            var unitProvider = new UnitsProviderFake();
            UnitRepository = unitProvider.GetUnitRepository();
            return this.WithService(UnitRepository);
        }

        public FakeServiceProviderBuilderForJob AddBuildingRepository()
        {
            return this.WithService(BuildingsProviderFake.GetBuildingRepository());
        }
    }
}
