using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TTB.DAL.Models;
using TTB.Gameplay.API;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TravianTelegramBot.Client
{
    public class GameplayClient : IGameplayClient
    {
        private readonly IGameplayApi _api;
        private readonly ILogger<GameplayClient> _logger;

        public GameplayClient(IGameplayApi api, ILogger<GameplayClient> logger)
        {
            _api = api;
            _logger = logger;
        }

        public Task<BaseScenarioResult> GetTargetsFromMessage(TravianUser user, string messageUrl)
        {
            var player = Map(user);
            return _api.GetTargetsFromMessage(player, messageUrl);
        }

        public async Task<BaseScenarioResult> Logout(TravianUser user)
        {
            var player = Map(user);
            return await _api.Logout(player);
        }

        public async Task<BaseScenarioResult> ExecuteActions<T>(TravianUser user, IEnumerable<T> actions) where T : GameAction
        {
            var player = Map(user);
            return await _api.RunScenarioWithActions(player, actions);
        }

        public async Task<BaseScenarioResult> RunScan(TravianUser user, bool details = false)
        {
            var player = Map(user);
            return await _api.Scan(player, details);
        }

        public async Task<BaseScenarioResult> RunUpdateUserInfo(TravianUser user)
        {
            var player = Map(user);
            return await _api.UpdateUserInfo(player);
        }

        public async Task<BaseScenarioResult> RunWatch(TravianUser user)
        {
            var player = Map(user);
            return await _api.Watch(player);
        }

        private Player Map(TravianUser user)
        {
            return new Player
            {
                UserName = user.UserName,
                Password = user.Password,
                TimeZone = user.PlayerData.TimeZone,
                Uri = new Uri(user.Url)
            };
        }

        public async Task<BaseScenarioResult> RunGetVillagesInfo(TravianUser user)
        {
            var player = Map(user);
            return await _api.GetVillagesInfo(player);
        }
    }
}
