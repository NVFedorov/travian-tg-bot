using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using TravianTelegramBot.Client;
using TTB.DAL.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TTB.Test.Fake
{
    public class GameplayClientFake : IGameplayClient
    {
        private readonly IMapper _mapper;

        public GameplayClientFake(IMapper mapper)
        {
            _mapper = mapper;
        }

        public Task<BaseScenarioResult> RunScan(TravianUser user, bool details = false)
        {
            var player = _mapper.Map<Player>(user);
            var result = BuildScanResult();
            result.Player = player;
            return Task.FromResult(result);
        }

        public Task<BaseScenarioResult> RunUpdateUserInfo(TravianUser user)
        {
            var player = _mapper.Map<Player>(user);
            var result = new BaseScenarioResult
            {
                Player = player
            };
            return Task.FromResult(result);
        }

        public Task<BaseScenarioResult> RunWatch(TravianUser user)
        {
            var player = _mapper.Map<Player>(user);
            var result = BuildWatchResult();
            result.Player = player;
            return Task.FromResult(result);
        }

        private BaseScenarioResult BuildWatchResult()
        {
            var watch = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.IsUserUnderAttack = true)
                .With(x => x.Scans = Builder<Incoming>
                    .CreateListOfSize(2)
                    .All()
                    .With(z => z.IntruderDetails = Builder<Village>.CreateNew().Build()).Build().ToList())
                .With(x => x.Success = true)
                .With(x => x.Villages = Builder<Village>.CreateListOfSize(2)
                    .All()
                    .With(y => y.Alliance = "TestAlliance")
                    .With(y => y.CoordinateX = new Random().Next(401) - 200)
                    .With(y => y.CoordinateY = new Random().Next(401) - 200)
                    .TheFirst(1)
                    .With(y => y.Attacks = Builder<Incoming>
                        .CreateListOfSize(2)
                        .All()
                        .With(z => z.VillageName = "test_village_under_attack")
                        .Build()
                        .ToList())
                    .Build().ToList())
                .Build();

            return watch;
        }

        private BaseScenarioResult BuildScanResult()
        {
            var scan = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.IsUserUnderAttack = true)
                .With(x => x.Success = true)
                .With(x => x.Villages = Builder<Village>.CreateListOfSize(2)
                    .All()
                    .With(y => y.Alliance = "TestAlliance")
                    .With(y => y.CoordinateX = new Random().Next(401) - 200)
                    .With(y => y.CoordinateY = new Random().Next(401) - 200)
                    .TheFirst(1)
                    .With(y => y.Attacks = Builder<Incoming>
                        .CreateListOfSize(2)
                        .All()
                        .With(z => z.VillageName = "test_village_under_attack")
                        .With(z => z.IntruderDetails = Builder<Village>.CreateNew().Build())
                        .TheFirst(1)
                        .With(z => z.IntruderVillageUrl = "test.coma")
                        .With(z => z.DateTime = FakeDataProvider.ExistingAttackDateTime)
                        .TheNext(1)
                        .With(z => z.DateTime = FakeDataProvider.NewAttackDateTime)
                        .Build()
                        .ToList())
                    .Build().ToList())
                .Build();

            return scan;
        }

        public Task<BaseScenarioResult> Logout(TravianUser user)
        {
            throw new NotImplementedException();
        }

        public Task<BaseScenarioResult> GetTargetsFromMessage(TravianUser user, string messageUrl)
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

            return Task.FromResult(result);
        }

        public Task<BaseScenarioResult> ExecuteActions<T>(TravianUser user, IEnumerable<T> actions) where T : GameAction
        {
            var player = _mapper.Map<Player>(user);
            var result = new BaseScenarioResult
            {
                Player = player
            };
            return Task.FromResult(result);
        }

        public Task<BaseScenarioResult> RunGetVillagesInfo(TravianUser user)
        {
            throw new NotImplementedException();
        }
    }
}
