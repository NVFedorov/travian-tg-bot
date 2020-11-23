using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TravianTelegramBot.Client
{
    public interface IGameplayClient
    {
        Task<BaseScenarioResult> RunWatch(TravianUser user);
        Task<BaseScenarioResult> RunScan(TravianUser user, bool details = false);
        Task<BaseScenarioResult> ExecuteActions<T>(TravianUser user, IEnumerable<T> actions) where T : GameAction;
        Task<BaseScenarioResult> RunUpdateUserInfo(TravianUser user);
        Task<BaseScenarioResult> Logout(TravianUser user);
        Task<BaseScenarioResult> GetTargetsFromMessage(TravianUser user, string messageUrl);
        Task<BaseScenarioResult> RunGetVillagesInfo(TravianUser user);
    }
}
