using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;
using TTB.DAL.Repository.Impl;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;

namespace TravianTelegramBot.Providers.Impl
{
    public class ActionProvider : IActionProvider
    {
        private const int _beforeAttackInterval = 30;
        private const string SendArmyKey = "send_to_any_natar";

        private readonly IUnitRepository _unitRepository;
        private readonly IVillageRepository _villageRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IBuildingPlanRepository _buildingPlanRepository;
        private readonly IMapper _mapper;

        public ActionProvider(
            IUnitRepository unitRepository,
            IVillageRepository villageRepository,
            IBuildingRepository buildingRepository,
            IBuildingPlanRepository buildingPlanRepository,
            IMapper mapper)
        {
            _unitRepository = unitRepository;
            _villageRepository = villageRepository;
            _buildingRepository = buildingRepository;
            _buildingPlanRepository = buildingPlanRepository;
            _mapper = mapper;
        }

        public async Task<(bool HasMoreToBuild, IEnumerable<BuildAction> Actions)> GetBuildActions(Village village, IEnumerable<BuildingModel> allBuildings = null)
        {
            var result = new List<BuildAction>();
            var buildings = (allBuildings ?? await _buildingRepository.GetAllBuildings()).ToList();
            var dorf1BuildingToBuild = GetResourceFieldToBuildNext(village, buildings);

            if (dorf1BuildingToBuild != null)
            {
                result.Add(new BuildAction
                {
                    BuildingId = dorf1BuildingToBuild.BuildingId,
                    Action = GameActionType.BUILD,
                    BuildingSlot = dorf1BuildingToBuild.Id,
                    Village = village
                });
            }

            BuildingPlanModel plan = null;
            if (!string.IsNullOrEmpty(village.BuildingPlanId))
                plan = await _buildingPlanRepository.GetBuildingPlan(village.BuildingPlanId);
            if (plan == null)
                plan = (await _buildingPlanRepository.GetBuildingPlans(BuildingPlanRepository.DefaultPlanUserName)).FirstOrDefault();

            var hasMoreBuildingsToBuild = TryGetNextBuildingInVillageToBuildFromBuildingPlan(village, buildings, plan, out var dorf2Building);
            if (dorf2Building != null)
            {
                var slot2 = village.BuildingSlots
                    .Where(x => x.BuildingId == dorf2Building.BuildingId)
                    .OrderBy(x => x.Level)
                    .FirstOrDefault();

                result.Add(new BuildAction
                {
                    BuildingId = dorf2Building.BuildingId,
                    Action = GameActionType.BUILD,
                    BuildingSlot = string.IsNullOrEmpty(slot2?.Id) ? dorf2Building.PrefferedBuildingSlot : slot2.Id,
                    Village = village
                });
            }

            return (hasMoreBuildingsToBuild || dorf1BuildingToBuild != null, result);
        }

        public async Task<IEnumerable<GameAction>> GetActionsForPlayer(TravianUser user)
        {
            var playerData = user.PlayerData;
            var villages = await _villageRepository.GetVillages(user.UserName);

            if (!villages.Any(x => x.IsSaveResourcesFeatureOn || x.IsSaveTroopsFeatureOn))
            {
                return null;
            }

            var capital = villages.FirstOrDefault(x => x.IsCapital);

            var result = new List<GameAction>();
            if (capital?.Attacks != null && capital.Attacks.Any())
            {
                if (capital.IsSaveTroopsFeatureOn)
                {
                    result.Add(new SendArmyAction
                    {
                        Action = GameActionType.SEND_ARMY,
                        Village = _mapper.Map<Village>(capital),
                        To = new Village
                        {
                            Name = SendArmyKey
                        },
                        SendAll = true,
                        Type = SendArmyType.RAID
                    });
                }
                if (capital.IsSaveResourcesFeatureOn)
                {
                    var capitalActions = await Task.WhenAll(
                        //CreateActionsForTroops(capital, playerData.Tribe, VillageActionType.UPGRADE_ARMY),
                        CreateActionsForTroops(capital, playerData.Tribe, GameActionType.TRAIN_ARMY));
                    result.AddRange(capitalActions);
                    // send resources is not needed
                }
            }

            // we don't send resources from capital for now
            result.AddRange(await CreateActionsForOtherVillages(villages, capital, playerData.Tribe));

            return result;
        }

        private async Task<TrainArmyAction> CreateActionsForTroops(VillageModel village, TTB.DAL.Models.GameModels.Enums.Tribe tribe, GameActionType actionType)
        {
            var units = new List<UnitModel>();
            if (village.PreferableUnits != null)
            {
                var searchTasks = new List<Task<UnitModel>>();
                foreach (var pref in village.PreferableUnits)
                {
                    searchTasks.Add(_unitRepository.GetUnit(pref, tribe));
                }

                units = (await Task.WhenAll(searchTasks)).ToList();
            }
            else
            {
                var createOffenceTroops = village.Types?.Contains(VillageType.OFFENCE);
                var unitsToCreate = createOffenceTroops.HasValue && createOffenceTroops.Value || village.IsCapital && !createOffenceTroops.HasValue
                    ? await _unitRepository.GetOffenceUnits(tribe)
                    : await _unitRepository.GetDeffenceUnits(tribe);
                units = unitsToCreate.ToList();
            }
            var action = new TrainArmyAction
            {
                Action = actionType,
                Village = _mapper.Map<Village>(village),
                UnitsToTrain = units.ToDictionary(x => x.LocalizedNameRu, x => (int)TrainArmyFlag.MAX)
            };

            return action;
        }

        private async Task<IEnumerable<GameAction>> CreateActionsForOtherVillages(IEnumerable<VillageModel> villages, VillageModel capital, TTB.DAL.Models.GameModels.Enums.Tribe tribe)
        {
            var villagesUnderAttack = villages.Where(x => x.Attacks != null && x.Attacks.Any() && !x.IsCapital);
            var trader = await _unitRepository.GetUnit("trader", tribe);
            var highPriorityReceivers = new List<VillageModel> { capital };
            var result = new List<GameAction>();

            foreach (var villageUnderAttack in villagesUnderAttack)
            {
                if (villageUnderAttack.IsSaveResourcesFeatureOn)
                {
                    var to = FindVillageToSendResources(villageUnderAttack, highPriorityReceivers, trader);
                    if (to == null)
                    {
                        to = FindVillageToSendResources(villageUnderAttack, villages, trader);
                    }

                    if (to != null)
                    {
                        result.Add(new SendResourcesAction
                        {
                            Action = GameActionType.SEND_RESOURCES,
                            Village = _mapper.Map<Village>(villageUnderAttack),
                            To = _mapper.Map<Village>(to)
                        });
                    }

                    var units = new List<UnitModel>();
                    if (villageUnderAttack.PreferableUnits != null)
                    {
                        var searchTasks = new List<Task<UnitModel>>();
                        foreach (var pref in villageUnderAttack.PreferableUnits)
                        {
                            searchTasks.Add(_unitRepository.GetUnit(pref, tribe));
                        }

                        units = (await Task.WhenAll(searchTasks)).ToList();
                    }
                    else
                    {
                        units = (await _unitRepository.GetUnitsByType(tribe, UnitType.FOOT_TROOPS))
                                                        .Where(x => x.Attack < x.DeffenceAgainstCavalry && x.Attack < x.DeffenceAgainstInfantry)
                                                        .ToList();
                    }

                    result.Add(new TrainArmyAction
                    {
                        Action = GameActionType.TRAIN_ARMY,
                        Village = _mapper.Map<Village>(villageUnderAttack),
                        UnitsToTrain = units.ToDictionary(x => x.LocalizedNameRu, x => (int)TrainArmyFlag.MAX)
                    });
                }

                if (villageUnderAttack.IsSaveTroopsFeatureOn)
                {
                    result.Add(new SendArmyAction
                    {
                        Action = GameActionType.SEND_ARMY,
                        Village = _mapper.Map<Village>(villageUnderAttack),
                        To = new Village
                        {
                            Name = SendArmyKey
                        },
                        SendAll = true,
                        Type = SendArmyType.RAID
                    });
                }
            }

            return result;
        }

        private VillageModel FindVillageToSendResources(VillageModel from, IEnumerable<VillageModel> candidates, UnitModel trader)
        {
            candidates = candidates.Where(x => x != null);
            var capital = candidates.FirstOrDefault(x => x.IsCapital);
            if (capital != null)
            {
                if (!capital.Attacks.Any())
                {
                    return capital;
                }

                var time = Calculator.Calculator.CalculateTimeForUnit(
                            from.CoordinateX,
                            from.CoordinateY,
                            capital.CoordinateX,
                            capital.CoordinateY,
                            trader);

                var arrival = DateTimeOffset.UtcNow + time;
                var attacksAfterArrival = from.Attacks.Where(x => x.DateTime > arrival);
                if (attacksAfterArrival.Any())
                {

                    var nearestToArrival = attacksAfterArrival.Select(x => x.DateTime).Min();
                    if (nearestToArrival - arrival > TimeSpan.FromMinutes(_beforeAttackInterval))
                    {
                        return capital;
                    }
                }
                else
                {
                    return capital;
                }
            }

            TimeSpan min = TimeSpan.MaxValue;
            VillageModel to = null;
            foreach (var candidate in candidates.Where(x => x.CoordinateX != from.CoordinateX && x.CoordinateY != from.CoordinateY && !x.IsCapital))
            {
                var send = true;
                var time = Calculator.Calculator.CalculateTimeForUnit(
                            from.CoordinateX,
                            from.CoordinateY,
                            candidate.CoordinateX,
                            candidate.CoordinateY,
                            trader);
                if (candidate.Attacks != null && candidate.Attacks.Any())
                {
                    var arrival = DateTimeOffset.UtcNow + time;
                    var nearestToArrival = from.Attacks
                        .Where(x => x.DateTime > arrival)
                        .Select(x => x.DateTime)
                        .Min();
                    send = nearestToArrival - arrival > TimeSpan.FromMinutes(_beforeAttackInterval);
                }

                if (time < min && send)
                {
                    to = candidate;
                    min = time;
                }

            }

            return to;
        }

        private static BuildingSlot GetResourceFieldToBuildNext(Village village, List<BuildingModel> allBuildings)
        {
            var result = string.Empty;
            var production = village.ResourcesProduction;
            var resources = village.Resources;

            var toBuild = village.BuildingSlots.Where(x => x.Level < 10 && x.State == BuildingSlotState.Good);
            if (toBuild.Any())
            {
                var cropLandBuilding = allBuildings.First(y => y.Name == "cropland");
                var cropLandSlots = toBuild.Where(x => x.BuildingId == cropLandBuilding.BuildingId);

                if (production.FreeCrop < 5 && cropLandSlots.Any())
                    return cropLandSlots.OrderBy(x => x.Level).First();

                var clayPitBuilding = allBuildings.First(y => y.Name == "clay_pit");
                var clayPitSlots = toBuild.Where(x => x.BuildingId == clayPitBuilding.BuildingId);
                var woodCutterBuilding = allBuildings.First(y => y.Name == "woodcutter");
                var woodCutterSlots = toBuild.Where(x => x.BuildingId == woodCutterBuilding.BuildingId);
                var ironMineBuilding = allBuildings.First(y => y.Name == "iron_mine");
                var ironMineSlots = toBuild.Where(x => x.BuildingId == ironMineBuilding.BuildingId);

                var diff = village.Warehourse / 8;

                if (clayPitSlots.Any() && (resources.Clay < resources.Lumber * 1.5 || resources.Clay < diff))
                    return clayPitSlots.OrderBy(x => x.Level).First();

                if (woodCutterSlots.Any() && (resources.Lumber < resources.Iron * 1.5 || resources.Lumber < resources.Clay * 1.5
                    || resources.Lumber < diff || resources.Lumber < diff))
                    return woodCutterSlots.OrderBy(x => x.Level).First();

                if (ironMineSlots.Any() && (resources.Iron * 1.5 < resources.Clay || resources.Iron < diff))
                    return ironMineSlots.OrderBy(x => x.Level).First();

                if ((production.Clay <= production.Lumber || production.Clay <= production.Iron) && clayPitSlots.Any())
                    return clayPitSlots.OrderBy(x => x.Level).First();

                if (production.Lumber * 1.2 <= production.Clay && woodCutterSlots.Any())
                    return woodCutterSlots.OrderBy(x => x.Level).First();

                if (production.Iron * 1.2 < production.Lumber && ironMineSlots.Any())
                    return ironMineSlots.OrderBy(x => x.Level).First();

                if ((production.Crop + production.FreeCrop) * 5 < production.Iron && cropLandSlots.Any())
                    return cropLandSlots.OrderBy(x => x.Level).First();

                if (production.Clay <= production.Lumber && clayPitSlots.Any())
                    return clayPitSlots.OrderBy(x => x.Level).First();
                if (production.Lumber <= production.Iron && woodCutterSlots.Any())
                    return woodCutterSlots.OrderBy(x => x.Level).First();
                if (ironMineSlots.Any())
                    return ironMineSlots.OrderBy(x => x.Level).First();
                if (cropLandSlots.Any())
                    return cropLandSlots.OrderBy(x => x.Level).First();
            }

            return null;
        }


        /// <summary>
        /// Tries to get the next building in village to build from building plan.
        /// </summary>
        /// <param name="village">The village.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="buildingModel">The building model.</param>
        /// <returns>True if building plan is not yet completed. Otherwise: false</returns>
        private static bool TryGetNextBuildingInVillageToBuildFromBuildingPlan(Village village, List<BuildingModel> buildings, BuildingPlanModel buildingPlan, out BuildingModel buildingModel)
        {
            //var warehouse = buildings.First(x => x.Name == "warehouse");
            //var granary = buildings.First(x => x.Name == "granary");
            //if (village.ResourcesProduction.ToList().Max() * 5 > village.Warehourse)
            //    return warehouse;
            //if (village.ResourcesProduction.Crop * 5 > village.Granary)
            //    return granary;

            var plan = CreateBuildingPlan(buildings, buildingPlan);
            var i = 0;
            var result = false;
            while (!result && i < plan.Count)
            {
                var currentStep = plan[i++];
                if (CheckPrerequiresites(currentStep.Item1, village, buildings))
                {
                    var slot = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == currentStep.Item1.BuildingId);
                    var level = slot?.Level ?? 0;
                    if (level < currentStep.Item2)
                    {
                        result = true;
                        var state = (slot?.State ?? BuildingSlotState.Empty);
                        var canBuild = state == BuildingSlotState.Good || state == BuildingSlotState.Empty;
                        if (canBuild)
                        {
                            buildingModel = currentStep.Item1;
                            return true;
                        }
                    }
                }
            }

            buildingModel = null;
            return result;
        }

        private static BuildingModel GetVillageFieldBuildingSlotToBuildNext(Village village, List<BuildingModel> buildings)
        {
            var mainBuilding = buildings.First(x => x.Name == "main_building");
            var mbLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == mainBuilding.BuildingId)?.Level ?? 0;
            if (mbLevel < 3)
                return mainBuilding;

            var warehouse = buildings.First(x => x.Name == "warehouse");
            var whLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == warehouse.BuildingId)?.Level ?? 0;
            if (whLevel < 3)
                return warehouse;

            var granary = buildings.First(x => x.Name == "granary");
            var grLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == granary.BuildingId)?.Level ?? 0;
            if (grLevel < 2)
                return granary;

            if (village.ResourcesProduction.ToList().Max() * 5 > village.Warehourse)
                return warehouse;
            if (village.ResourcesProduction.Crop * 5 > village.Granary)
                return granary;

            if (whLevel < 13)
                return warehouse;

            if (grLevel < 9)
                return granary;

            if (mbLevel < 20)
                return mainBuilding;

            var marketplace = buildings.First(x => x.Name == "marketplace");
            var mpLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == marketplace.BuildingId)?.Level ?? 0;
            if (mpLevel < 4)
                return marketplace;

            var rally_point = buildings.First(x => x.Name == "rally_point");
            var rpLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == rally_point.BuildingId)?.Level ?? 0;
            if (rpLevel < 1)
                return rally_point;

            var barracks = buildings.First(x => x.Name == "barracks");
            var barracksLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == barracks.BuildingId)?.Level ?? 0;
            if (barracksLevel < 4)
                return barracks;

            var academy = buildings.First(x => x.Name == "academy");
            var academyLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == academy.BuildingId)?.Level ?? 0;
            if (academyLevel < 6)
                return academy;

            var smithy = buildings.First(x => x.Name == "smithy");
            var smithyLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == smithy.BuildingId)?.Level ?? 0;
            if (smithyLevel < 4)
                return smithy;

            if (mpLevel < 15)
                return marketplace;

            var stable = buildings.First(x => x.Name == "stable");
            var stableLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == stable.BuildingId)?.Level ?? 0;
            if (stableLevel < 11)
                return stable;

            if (mpLevel < 20)
                return marketplace;

            if (whLevel < 20)
                return warehouse;

            if (grLevel < 13)
                return granary;

            var trade_office = buildings.First(x => x.Name == "trade_office");
            var toLevel = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == trade_office.BuildingId)?.Level ?? 0;
            if (toLevel < 10)
                return trade_office;

            return null;
        }

        private static SortedList<int, Tuple<BuildingModel, int>> CreateBuildingPlan(List<BuildingModel> buildings, BuildingPlanModel buildingPlan)
        {
            var result = new SortedList<int, Tuple<BuildingModel, int>>();

            if (buildingPlan == null)
            {
                var mainBuilding = buildings.First(x => x.Name == "main_building");
                var warehouse = buildings.First(x => x.Name == "warehouse");
                var granary = buildings.First(x => x.Name == "granary");
                var marketplace = buildings.First(x => x.Name == "marketplace");
                var rally_point = buildings.First(x => x.Name == "rally_point");
                var barracks = buildings.First(x => x.Name == "barracks");
                var academy = buildings.First(x => x.Name == "academy");
                var smithy = buildings.First(x => x.Name == "smithy");
                var stable = buildings.First(x => x.Name == "stable");
                var trade_office = buildings.First(x => x.Name == "trade_office");
                var sawmill = buildings.First(x => x.Name == "sawmill");
                var brickyard = buildings.First(x => x.Name == "brickyard");
                var iron_foundry = buildings.First(x => x.Name == "iron_foundry");
                var grain_mill = buildings.First(x => x.Name == "grain_mill");
                var bakery = buildings.First(x => x.Name == "bakery");
                var residence = buildings.First(x => x.Name == "residence");

                var steps = new List<Tuple<BuildingModel, int>>
            {
                new Tuple<BuildingModel, int>(mainBuilding, 3),
                new Tuple<BuildingModel, int>(warehouse, 3),
                new Tuple<BuildingModel, int>(granary, 2),
                new Tuple<BuildingModel, int>(mainBuilding, 12),
                new Tuple<BuildingModel, int>(warehouse, 12),
                new Tuple<BuildingModel, int>(granary, 9),
                new Tuple<BuildingModel, int>(residence, 3),
                new Tuple<BuildingModel, int>(mainBuilding, 20),
                new Tuple<BuildingModel, int>(marketplace, 3),
                new Tuple<BuildingModel, int>(barracks, 3),
                new Tuple<BuildingModel, int>(academy, 5),
                new Tuple<BuildingModel, int>(smithy, 3),
                new Tuple<BuildingModel, int>(stable, 5),
                new Tuple<BuildingModel, int>(brickyard, 2),
                new Tuple<BuildingModel, int>(sawmill, 2),
                new Tuple<BuildingModel, int>(brickyard, 3),
                new Tuple<BuildingModel, int>(sawmill, 3),
                new Tuple<BuildingModel, int>(iron_foundry, 2),
                new Tuple<BuildingModel, int>(brickyard, 5),
                new Tuple<BuildingModel, int>(sawmill, 5),
                new Tuple<BuildingModel, int>(iron_foundry, 5),
                new Tuple<BuildingModel, int>(warehouse, 20),
                new Tuple<BuildingModel, int>(granary, 20),
                new Tuple<BuildingModel, int>(marketplace, 20),
                new Tuple<BuildingModel, int>(stable, 10),
                new Tuple<BuildingModel, int>(trade_office, 12),
                new Tuple<BuildingModel, int>(grain_mill, 5),
                new Tuple<BuildingModel, int>(bakery, 5),
                new Tuple<BuildingModel, int>(residence, 10)
            };

                for (var i = 0; i < steps.Count; i++)
                {
                    result.Add(i, steps[i]);
                }
            }
            else
            {
                foreach (var step in buildingPlan.BuildingSteps)
                {
                    result.Add(step.Order, new Tuple<BuildingModel, int>(buildings.FirstOrDefault(x => x.BuildingId == step.BuildingId), step.Level));
                }
            }

            return result;
        }

        private static bool CheckPrerequiresites(BuildingModel building, Village village, List<BuildingModel> allBuildings)
        {
            if (!building.Prerequiresites?.Any() ?? true)
                return true;

            foreach (var prereq in building.Prerequiresites)
            {
                var prereqBuilding = allBuildings.FirstOrDefault(x => x.Name == prereq.Name);
                var villageBuilding = village.BuildingSlots.FirstOrDefault(x => x.BuildingId == prereqBuilding?.BuildingId);
                if (villageBuilding == null && prereq.Level < 1)
                    return true;

                if (villageBuilding == null || villageBuilding.Level < prereq.Level)
                    return false;
            }

            return true;
        }
    }
}
