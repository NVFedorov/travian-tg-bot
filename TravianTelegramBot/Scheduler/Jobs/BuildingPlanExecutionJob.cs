using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Services;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class BuildingPlanExecutionJob : AbstractJob
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IVillageRepository _villageRepository;
        private readonly IMapper _mapper;
        private readonly IActionProvider _actionProvider;
        private readonly ICommandFactory _commandFactory;
        private readonly IUnitRepository _unitRepository;

        public BuildingPlanExecutionJob(IServiceProvider service) : base(service)
        {
            _buildingRepository = service.GetService<IBuildingRepository>();
            _villageRepository = service.GetService<IVillageRepository>();
            _mapper = service.GetService<IMapper>();
            _actionProvider = service.GetService<IActionProvider>();
            _commandFactory = service.GetService<ICommandFactory>();
            _unitRepository = service.GetService<IUnitRepository>();
        }

        protected override async Task ExecuteJob(JobExecutionData jobExecutionData)
        {
            var allVillages = await _villageRepository.GetVillages(_travianUser.UserName);
            var buildVillages = allVillages
                .Where(x => x.IsBuildFeatureOn)
                .ToList();

            if (!buildVillages.Any())
            {
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"No villages with build feature found for player {_travianUser.UserName}");
                return;
            }

            foreach (var village in buildVillages.Where(x => x.IsWaitingForResources))
            {
                // for villages waiting for resources next execution time is the time when resources must be delivered
                // if next execution time passed resources are expected to be delivered
                village.IsWaitingForResources = village.NextBuildingPlanExecutionTime.HasValue && village.NextBuildingPlanExecutionTime.Value > DateTimeOffset.Now;
            }

            BaseScenarioResult infos = null;
            try
            {
                var updateActions = buildVillages
                    .Select(x => new GameAction { Village = _mapper.Map<Village>(x) });
                infos = await _gameplayClient.ExecuteActions(_travianUser, updateActions);
                var update = infos.Villages.Select(x => _mapper.Map<VillageModel>(x));
                await _villageRepository.UpdateWatchInfo(update);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"Unable to update villages info for player {_travianUser.UserName}");
                return;
            }

            if (!infos?.Villages?.Any() ?? true)
            {
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"No info was found for villages of {_travianUser.UserName}");
                return;
            }

            // suits only for rome or for player with travian plus. TODO: fix for other tribes
            var freeVillages = infos.Villages
                .Where(x => x.CanBuild)
                .ToList();

            var busyVillages = infos.Villages
                .Where(x => !x.CanBuild)
                .ToList();

            if (busyVillages?.Any() ?? false)
            {
                foreach (var village in busyVillages)
                {
                    // if village has less than 10% filled warehouse we should send there more resources
                    var limit = village.Warehourse * 0.1;
                    if (!village.Dorf1BuildTimeLeft.HasValue || village.Resources.Lumber < limit || village.Resources.Clay < limit
                        || village.Resources.Iron < limit || village.Resources.Crop < village.Granary * 0.1)
                    {
                        await SendResources(allVillages.ToList(), village);
                    }
                }
            }

            var result = new BaseScenarioResult();

            if (!freeVillages?.Any() ?? true)
            {
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"All villages with building feature of player {_travianUser.UserName} are busy.");
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"Calculating next build command execution time for player {_travianUser.UserName}.");
            }
            else
            {
                var i = 0;
                var allBuidlgins = await _buildingRepository.GetAllBuildings();
                while ((freeVillages?.Any() ?? false) && i++ < buildVillages.Count)
                {
                    var allActions = new List<BuildAction>();
                    foreach (var village in freeVillages)
                    {
                        var (hasMoreToBuild, actions) = await _actionProvider.GetBuildActions(village, allBuidlgins);
                        if (!hasMoreToBuild)
                        {
                            await _bot.SendTextMessageAsync(_botUser.ChatId, $"No actions needed for village {village.Name} of player {village.PlayerName}. Swithching off the Build Feature.");
                            try
                            {
                                var villageToUpdate = await _villageRepository.GetVillage(village.CoordinateX, village.CoordinateY);
                                villageToUpdate.IsBuildFeatureOn = false;
                                await _villageRepository.UpdateInfos(new List<VillageModel> { villageToUpdate });
                            }
                            catch (Exception exc)
                            {
                                _logger.LogError(exc, exc.Message);
                                await _bot.SendTextMessageAsync(_botUser.ChatId, $"Unable to update {village.Name} of player {village.PlayerName}. Check Logs.");
                            }
                        }
                        else
                        {
                            allActions.AddRange(actions);
                        }
                    }

                    if (allActions.Any())
                    {
                        result = await _gameplayClient.ExecuteActions(_travianUser, allActions);

                        var buildErrors = result.Errors
                            .Where(x => x is BuildScenarioError)
                            .Cast<BuildScenarioError>();
                        if (buildErrors.Any())
                        {
                            var notEnoughRes = buildErrors
                                .Where(x => (x as BuildScenarioError).BuildErrorType == BuildErrorType.NotEnoughResources);
                            if (notEnoughRes.Any())
                            {
                                foreach (var n in notEnoughRes)
                                {
                                    var updatedVillage = result.Villages.First(x => x.CoordinateX == n.Village.CoordinateX && x.CoordinateY == n.Village.CoordinateY);
                                    await SendResources(allVillages.ToList(), updatedVillage);
                                }
                            }

                            var noSpaceInQueue = buildErrors
                                .Where(x => (x as BuildScenarioError).BuildErrorType == BuildErrorType.NoSpaceInQueue);
                            if (noSpaceInQueue.Any())
                            {
                                foreach (var n in noSpaceInQueue)
                                {
                                    var errorVillage = result.Villages.First(x => x.CoordinateX == n.Village.CoordinateX && x.CoordinateY == n.Village.CoordinateY);
                                    errorVillage.CanBuild = false;
                                }
                            }
                        }

                        freeVillages = result.Villages.Where(x => x.CanBuild).ToList();
                    }
                }
            }

            var max = infos.Villages.Count > result.Villages.Count ? infos.Villages.Count : result.Villages.Count;
            TimeSpan nextRun = TimeSpan.MaxValue;
            for (var j = 0; j < max; j++)
            {
                if (j < infos.Villages.Count)
                {
                    if (infos.Villages[j].Dorf1BuildTimeLeft.HasValue
                        && infos.Villages[j].Dorf1BuildTimeLeft.Value > TimeSpan.Zero
                        && infos.Villages[j].Dorf1BuildTimeLeft.Value < nextRun)
                        nextRun = infos.Villages[j].Dorf1BuildTimeLeft.Value;
                }
                if (j < result.Villages.Count)
                {
                    if (result.Villages[j].Dorf1BuildTimeLeft.HasValue
                        && result.Villages[j].Dorf1BuildTimeLeft.Value > TimeSpan.Zero
                        && result.Villages[j].Dorf1BuildTimeLeft.Value < nextRun)
                        nextRun = result.Villages[j].Dorf1BuildTimeLeft.Value;
                }
            }

            if (nextRun < TimeSpan.MaxValue)
            {
                var waitingForResources = buildVillages
                    ?.Where(x => x.IsWaitingForResources && x.NextBuildingPlanExecutionTime.HasValue)
                    ?.Select(x => x.NextBuildingPlanExecutionTime.Value)
                    ?.ToList();

                var nextRunDateTime = DateTimeOffset.Now + nextRun;
                var nearestResourcesDelivery = (waitingForResources?.Any() ?? false) ? waitingForResources.Min() : DateTimeOffset.MaxValue;

                var cmd = _commandFactory.GetQueueableCommand(nameof(BuildCommand), _botUser.ChatId);
                cmd.Start = (nextRunDateTime < nearestResourcesDelivery ? nextRunDateTime : nearestResourcesDelivery) + TimeSpan.FromSeconds(2);
                await cmd.Execute();
            }
            else
            {
                await _bot.SendTextMessageAsync(_botUser.ChatId, $"Unable to calculate next build command execution time for player {_travianUser.UserName}");
            }
        }

        private async Task SendResources(List<VillageModel> allVillages, Village target)
        {
            var village = allVillages.First(x => x.CoordinateX == target.CoordinateX && x.CoordinateY == target.CoordinateY);
            if (!village.IsWaitingForResources)
            {
                var nearest = Calculator.Calculator.GetNearestVillage(allVillages.Where(x => !x.IsCapital && !x.IsBuildFeatureOn), village);
                var action = new SendResourcesAction
                {
                    Village = _mapper.Map<Village>(nearest),
                    To = target,
                    Action = GameActionType.SEND_RESOURCES,
                    Resources = new Resources
                    {
                        Lumber = target.Warehourse - target.Resources.Lumber - target.ResourcesProduction.Lumber,
                        Clay = target.Warehourse - target.Resources.Clay - target.ResourcesProduction.Clay,
                        Iron = target.Warehourse - target.Resources.Iron - target.ResourcesProduction.Iron,
                        Crop = target.Granary - target.Resources.Crop - target.ResourcesProduction.Crop,
                    }
                };

                var sendResult = await _gameplayClient.ExecuteActions(_travianUser, new List<SendResourcesAction> { action });
                if (!sendResult.Errors.Any())
                {
                    var trader = await _unitRepository.GetTrader(_travianUser.PlayerData.Tribe);
                    var traderReachingTime = Calculator.Calculator.CalculateTimeForUnit(action.Village, action.To, trader);
                    var villageToUpdate = await _villageRepository.GetVillage(village.CoordinateX, village.CoordinateY);
                    villageToUpdate.NextBuildingPlanExecutionTime = DateTimeOffset.Now + traderReachingTime + TimeSpan.FromSeconds(2);
                    villageToUpdate.IsWaitingForResources = true;
                    await _villageRepository.UpdateWatchInfo(new List<VillageModel> { villageToUpdate });
                }
            }
        }
    }
}
