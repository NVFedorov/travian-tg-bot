using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using TTB.DAL.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TravianTelegramBot.Client
{
    public class GameplayClientFake : IGameplayClient
    {
        private readonly IMapper _mapper;
        private readonly ILogger<GameplayClientFake> _logger;

        public GameplayClientFake(IMapper mapper, ILogger<GameplayClientFake> logger)
        {
            _mapper = mapper;
            _logger = logger;
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

        public async Task<BaseScenarioResult> RunWatch(TravianUser user)
        {
            await Task.Delay(10000);
            var player = _mapper.Map<Player>(user);
            var result = BuildWatchResult();
            result.Player = player;
            _logger.LogDebug($"Fake watch command completed");
            return result;
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
                    .TheFirst(1)
                    .With(y => y.Name = "TestVillage1")
                    .With(y => y.CoordinateX = -10)
                    .With(y => y.CoordinateY = -10)
                    .With(y => y.Attacks = Builder<Incoming>
                        .CreateListOfSize(2)
                        .All()
                        .With(z => z.VillageName = "test_village_under_attack")
                        .With(z => z.IntruderDetails = Builder<Village>.CreateNew().Build())
                        .TheFirst(1)
                        .With(z => z.IntruderVillageUrl = "test.coma")
                        .With(z => z.DateTime = DateTimeOffset.UtcNow.AddMinutes(6))
                        .TheNext(1)
                        .With(z => z.DateTime = DateTimeOffset.UtcNow.AddMinutes(2))
                        .Build()
                        .ToList())
                    .TheNext(1)
                    .With(y => y.Name = "TestVillage2")
                    .With(y => y.CoordinateX = -5)
                    .With(y => y.CoordinateY = -5)
                    .Build().ToList())
                .Build();

            return scan;
        }

        public Task<BaseScenarioResult> Logout(TravianUser user)
        {
            return Task.FromResult(new BaseScenarioResult());
        }

        public Task<BaseScenarioResult> GetTargetsFromMessage(TravianUser user, string messageUrl)
        {
            return Task.FromResult(new BaseScenarioResult());
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
