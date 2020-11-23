using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TTB.Common.Settings;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize]
    public class KnowledgeBaseController : Controller
    {
        private readonly ITravianUserRepository _travianUserRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly ILogger<KnowledgeBaseController> _logger;

        public KnowledgeBaseController(
            ITravianUserRepository travianUserRepository, 
            IUnitRepository unitRepository, 
            IBuildingRepository buildingRepository,
            ILogger<KnowledgeBaseController> logger)
        {
            _travianUserRepository = travianUserRepository;
            _unitRepository = unitRepository;
            _buildingRepository = buildingRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Units()
        {
            try
            {
                var model = await _unitRepository.GetAllUnits();
                if (model == null)
                {
                    ViewData["Error"] = "Unable to get units from database:";
                }
                else
                {
                    ViewData["Error"] = TempData["Error"];
                }

                model = model.OrderBy(x => x.Tribe).ThenBy(x => x.UnitType);
                ViewData["Error"] = TempData["Error"];
                return View("~/Views/KnowledgeBase/UnitsList.cshtml", model);
            }
            catch (Exception exc)
            {
                ViewData["Error"] = $"Unable to get units from database: {TempData["Error"]}";
                _logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to find units: {exc.Message}");
            }

            return View("~/Views/KnowledgeBase/UnitsList.cshtml");
        }

        public async Task<IActionResult> DefUnits()
        {
            //var tuser = await _travianUserRepository.GetActiveUser(User.Identity.Name);
            var model = await _unitRepository.GetDeffenceUnits(Tribe.GAUL);

            return View("~/Views/KnowledgeBase/UnitsList.cshtml", model);
        }

        public async Task<IActionResult> OffUnits()
        {
            //var tuser = await _travianUserRepository.GetActiveUser(User.Identity.Name);
            var model = await _unitRepository.GetOffenceUnits(Tribe.GAUL);

            return View("~/Views/KnowledgeBase/UnitsList.cshtml", model);
        }

        public async Task<IActionResult> UploadUnits(IFormFile formFile)
        {
            await UpdateKnowledgeCollection(formFile, _unitRepository);
            return RedirectToAction("Units");
        }

        public async Task<IActionResult> Buildings()
        {
            try
            {
                var model = await _buildingRepository.GetAllBuildings();
                if (model == null)
                {
                    ViewData["Error"] = "Unable to get buildings from database:";
                }
                else
                {
                    ViewData["Error"] = TempData["Error"];
                }

                model = model.OrderBy(x => x.Name);
                ViewData["Error"] = TempData["Error"];
                return View("~/Views/KnowledgeBase/BuildingsList.cshtml", model);
            }
            catch (Exception exc)
            {
                ViewData["Error"] = $"Unable to get buildings from database: {TempData["Error"]}";
                _logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to find buildings: {exc.Message}");
            }

            return View("~/Views/KnowledgeBase/BuildingsList.cshtml");
        }

        public async Task<IActionResult> UploadBuildings(IFormFile formFile)
        {
            await UpdateKnowledgeCollection(formFile, _buildingRepository);
            return RedirectToAction("Buildings");
        }

        private async Task UpdateKnowledgeCollection<T>(IFormFile formFile, T repo) where T : IKnowledgeRepository
        {
            try
            {
                if (formFile.Length > 0 && formFile.Length < 100 * 1024)
                {
                    var result = new StringBuilder();
                    using (var reader = new StreamReader(formFile.OpenReadStream()))
                    {
                        while (reader.Peek() >= 0)
                            result.AppendLine(reader.ReadLine());
                    }

                    await repo.UpdateCollection(result.ToString());
                }
            }
            catch (Exception exc)
            {
                TempData["Error"] = "Unable to update collection";
                _logger.LogError(LoggingEvents.UpdateItemException, exc, $"Unable to update collection of type {typeof(T)}: {exc.Message}");
            }
        }
    }
}