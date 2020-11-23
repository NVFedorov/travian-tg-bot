using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using TravianTelegramBot.ViewModels;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Repository;
using TTB.DAL.Repository.Impl;

namespace TravianTelegramBot.Controllers
{
    [Authorize()]
    public class BuildingPlanController : Controller
    {
        private readonly IBuildingPlanRepository _buildingPlanRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BuildingPlanController> _logger;

        public BuildingPlanController(
            IBuildingPlanRepository buildingPlanRepository,
            IBuildingRepository buildingRepository,
            IMapper mapper,
            ILogger<BuildingPlanController> logger)
        {
            _buildingPlanRepository = buildingPlanRepository;
            _buildingRepository = buildingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> List()
        {
            IEnumerable<BuildingPlanViewModel> model = null;
            try
            {
                var buildingPlans = (await _buildingPlanRepository.GetBuildingPlans(User.Identity.Name)).ToList();
                var @default = await _buildingPlanRepository.GetBuildingPlans(BuildingPlanRepository.DefaultPlanUserName);
                @default.ToList().ForEach(x => x.BotUserName = string.Empty);
                buildingPlans.AddRange(@default);
                model = buildingPlans.Select(x => _mapper.Map<BuildingPlanViewModel>(x));
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemException, exc, exc.Message);
            }

            return View(model);
        }

        [HttpGet]
        //[Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> Upsert(string id)
        {
            BuildingPlanViewModel model = null;
            try
            {
                var allBuildings = (await _buildingRepository.GetAllBuildings()).ToList();

                BuildingPlanModel plan;
                IEnumerable<BuildingPlanStepViewModel> queue = null;
                if (string.IsNullOrEmpty(id))
                {
                    plan = new BuildingPlanModel
                    {
                        BotUserName = User.Identity.Name
                    };
                }
                else
                {
                    plan = await _buildingPlanRepository.GetBuildingPlan(id);
                    queue = plan.BuildingSteps.Select(x => new BuildingPlanStepViewModel
                    {
                        Order = x.Order,
                        Level = x.Level,
                        Building = allBuildings.FirstOrDefault(y => y.BuildingId == x.BuildingId)
                    });
                }

                model = _mapper.Map<BuildingPlanViewModel>(plan);
                model.BuildingSteps = queue?.ToList();
                ViewData["buildings"] = allBuildings;
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemException, exc, exc.Message);
                ViewBag.ErrorMessage = "Unable to find Building Plan.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(BuildingPlanViewModel model)
        {
            try
            {
                var plan = _mapper.Map<BuildingPlanModel>(model);
                plan.BuildingSteps = model.BuildingSteps.Select(x => new BuildingPlanStepModel
                {
                    Order = x.Order,
                    Level = x.Level,
                    BuildingId = x.Building.BuildingId
                }).ToList();

                if (plan.BotUserName == BuildingPlanRepository.DefaultPlanUserName)
                    plan._id = string.Empty;

                plan.BotUserName = User.Identity.Name;

                if (string.IsNullOrEmpty(plan._id))
                    await _buildingPlanRepository.Insert(plan);
                else
                    await _buildingPlanRepository.Update(plan);

                var allBuildings = (await _buildingRepository.GetAllBuildings()).ToList();
                ViewData["buildings"] = allBuildings;
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.InsertItemException, exc, exc.Message);
                ViewBag.ErrorMessage = "Unable to update Building Plan.";
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _buildingPlanRepository.Delete(id);
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.DeleteItemException, exc, exc.Message);
                ViewBag.ErrorMessage = "Unable to delete Building Plan.";
            }

            return RedirectToAction("List");
        }
    }
}