using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TTB.Gameplay.API
{
    public interface IGameplayApi
    {
        Task<BaseScenarioResult> GetVillagesInfo(Player player);
        Task<BaseScenarioResult> Watch(Player player);
        Task<BaseScenarioResult> Scan(Player player, bool details);
        Task<BaseScenarioResult> UpdateUserInfo(Player player);
        Task<BaseScenarioResult> Logout(Player player);
        Task<BaseScenarioResult> GetTargetsFromMessage(Player player, string messageUrl);
        Task<BaseScenarioResult> RunScenarioWithActions<T>(Player player, IEnumerable<T> actions) where T : GameAction;
    }
}
