using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.ViewModels;
using TTB.Common.Settings;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize()]
    public class VillagesController : Controller
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ITravianUserRepository _travianUserRepository;
        private readonly IVillageRepository _villageRepository;
        private readonly IBuildingPlanRepository _buildingPlanRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<VillagesController> _logger;

        public VillagesController(
            IUnitRepository unitRepository,
            ITravianUserRepository travianUserRepository,
            IVillageRepository villageRepository,
            IBuildingPlanRepository buildingPlanRepository,
            IMapper mapper,
            ILogger<VillagesController> logger)
        {
            _unitRepository = unitRepository;
            _travianUserRepository = travianUserRepository;
            _villageRepository = villageRepository;
            _buildingPlanRepository = buildingPlanRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [Route("[controller]/{username}")]
        public async Task<IActionResult> Index(string username)
        {
            try
            {
                var villages = await _villageRepository.GetVillages(username);
                var model = villages.Select(x => _mapper.Map<VillageViewModel>(x));
                ViewData["username"] = username;
                return View(model);
            }
            catch (Exception exc)
            {
                var msg = "Unable to find villages.";
                ViewBag.ErrorMessage = msg;
                _logger.LogError(LoggingEvents.GetItemNotFound, exc, msg);
            }

            return View();
        }

        [HttpPost]
        [Route("[controller]/[action]")]
        public async Task<IActionResult> UpdateFeatures([FromBody] VillageViewModel model)
        {
            var msg = string.Empty;
            try
            {
                var villages = await _villageRepository.GetVillages(model.PlayerName);
                var village = villages.FirstOrDefault(x => x.VillageId == model.VillageId);
                if (village != null)
                {
                    village.IsSaveResourcesFeatureOn = model.IsSaveResourcesFeatureOn;
                    village.IsSaveTroopsFeatureOn = model.IsSaveTroopsFeatureOn;
                    village.IsSpamFeatureOn = model.IsSpamFeatureOn;
                    village.IsBuildFeatureOn = model.IsBuildFeatureOn;
                    village.PreferableUnits = model.PreferableUnits;
                    village.SpamUnits = model.SpamUnits.Where(x => x.Value > 0 && x.Value < 1000).ToDictionary(x => x.Key, x => x.Value);
                    village.BuildingPlanId = model.BuildingPlanId;
                    if (model.IsDeffence || model.IsOffence || model.IsResource || model.IsScan)
                    {
                        var types = new List<VillageType>();
                        if (model.IsDeffence)
                            types.Add(VillageType.DEFFENCE);
                        if (model.IsResource)
                            types.Add(VillageType.RESOURCES);
                        if (model.IsScan)
                            types.Add(VillageType.SCAN);
                        if (model.IsOffence)
                            types.Add(VillageType.OFFENCE);
                        village.Types = types;
                    }

                    await _villageRepository.UpdateInfos(new List<VillageModel> { village });

                    return Ok(new { result = "Features have been updated." });
                }
                else
                {
                    msg = "Unable to find village.";
                    _logger.LogError(LoggingEvents.GetItemNotFound, $"{msg} Village ID: [{model.VillageId}], Village Name: [{model.VillageName}]");
                }
            }
            catch (Exception exc)
            {
                msg = "Unable to update village features.";
                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message + $". Unable to find village. Village ID: [{model.VillageId}], Village Name: [{model.VillageName}]");
            }

            return BadRequest(new
            {
                result = msg
            });
        }

        [Route("[controller]/[action]/{username}")]
        public async Task<IActionResult> Units(string username, [FromQuery]string villageId)
        {
            var user = await _travianUserRepository.GetUserByName(username, User.Identity.Name);
            var units = await _unitRepository.GetUnitsByTribe(user.PlayerData.Tribe);
            var village = await _villageRepository.GetVillage(villageId);

            units = units.Where(x => x.UnitType != UnitType.EXPANSION && x.Name != "trader");
            ViewData["villageId"] = villageId;
            ViewData["username"] = username;
            ViewData["selected"] = village.PreferableUnits;
            return View(units.OrderBy(x => x.UnitType));
        }

        [HttpPost]
        [Route("[controller]/[action]/{villageId}")]
        public async Task<IActionResult> UpdatePreferences(string villageId, [FromBody]string[] unitNames)
        {
            try
            {
                var village = await _villageRepository.GetVillage(villageId);
                if (village != null)
                {
                    village.PreferableUnits = unitNames.ToList();
                    await _villageRepository.UpdateInfos(new List<VillageModel> { village });
                }
                else
                {
                    _logger.LogError(LoggingEvents.GetItemNotFound, $"Unable to find village by id: {villageId}");
                    return BadRequest($"Unable to find village by id: {villageId}");
                }
            }
            catch (Exception exc)
            {

                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message);
                return BadRequest($"Unable to find village by id: {villageId}");
            }

            return Ok(new
            {
                result = "Preffered units have been updated."
            });
        }

        [Route("[controller]/[action]/{username}")]
        public async Task<IActionResult> SpamUnits(string username, [FromQuery]string villageId)
        {
            var user = await _travianUserRepository.GetUserByName(username, User.Identity.Name);
            var units = await _unitRepository.GetUnitsByTribe(user.PlayerData.Tribe);
            var village = await _villageRepository.GetVillage(villageId);

            units = units.Where(x => x.UnitType != UnitType.EXPANSION && x.Name != "trader");
            ViewData["villageId"] = villageId;
            ViewData["username"] = username;
            ViewData["selected"] = village.SpamUnits;
            return View(units.OrderBy(x => x.UnitType));
        }

        [HttpPost]
        [Route("[controller]/[action]/{villageId}")]
        public async Task<IActionResult> UpdateSpamUnits(string villageId, [FromBody]IDictionary<string, int> units)
        {
            // security violation : check if the village belongs to user.
            try
            {
                var village = await _villageRepository.GetVillage(villageId);
                if (village != null)
                {
                    village.SpamUnits = units;
                    await _villageRepository.UpdateInfos(new List<VillageModel> { village });
                }
                else
                {
                    _logger.LogError(LoggingEvents.GetItemNotFound, $"Unable to find village by id: {villageId}");
                    return BadRequest($"Unable to find village by id: {villageId}");
                }
            }
            catch (Exception exc)
            {

                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message);
                return BadRequest($"Unable to find village by id: {villageId}");
            }

            return Ok(new
            {
                result = "Preffered units have been updated."
            });
        }

        [Route("[controller]/[action]/{username}")]
        public async Task<IActionResult> Settings(string username, [FromQuery] string villageId)
        {
            VillageViewModel model = null;
            try
            {
                var villages = await _villageRepository.GetVillages(username);
                var village = villages.FirstOrDefault(x => x.VillageId == villageId); //await _villageRepository.GetVillage(villageId);
                if (village.PlayerName != username)
                {
                    return NotFound($"Unable to find village [{villageId}] for travian user [{username}]");
                }

                var user = await _travianUserRepository.GetUserByName(username, User.Identity.Name);
                var units = await _unitRepository.GetUnitsByTribe(user.PlayerData.Tribe);
                units = units.Where(x => x.UnitType != UnitType.EXPANSION && x.Name != "trader");
                var buildingPlans = await _buildingPlanRepository.GetBuildingPlans(User.Identity.Name);
                ViewData["buildingPlans"] = buildingPlans;
                ViewData["villageId"] = villageId;
                ViewData["username"] = username;
                ViewData["units"] = units;
                ViewData["trainUnits"] = village.PreferableUnits;
                ViewData["spamUnits"] = village.SpamUnits;
                ViewData["timeZone"] = string.IsNullOrEmpty(user.PlayerData.TimeZone) ? "UTC+0" : user.PlayerData.TimeZone;

                model = _mapper.Map<VillageViewModel>(village);
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message);
            }

            return View(model);
        }

        [Route("[controller]/[action]/{username}")]
        public async Task<IActionResult> CleanVillages(string username)
        {
            try
            {
                await _villageRepository.DeleteVillages(username);
            }
            catch(Exception exc)
            {
                _logger.LogError(LoggingEvents.DeleteItemException, exc, exc.Message);
            }

            return RedirectToAction("Index", new { username });
        }
    }
}