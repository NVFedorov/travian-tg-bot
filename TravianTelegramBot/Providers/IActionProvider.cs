using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;

namespace TravianTelegramBot.Providers
{
    public interface IActionProvider
    {
        Task<IEnumerable<GameAction>> GetActionsForPlayer(TravianUser user);
        Task<(bool HasMoreToBuild, IEnumerable<BuildAction> Actions)> GetBuildActions(Village village, IEnumerable<BuildingModel> allBuildings = null);
    }
}
