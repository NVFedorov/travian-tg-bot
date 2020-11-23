using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Scheduler;
using TravianTelegramBot.Services;
using TravianTelegramBot.ViewModels;
using TTB.Common.Settings;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize()]
    public class JobManagerController : Controller
    {
        private readonly UserManager<BotUser> _userManager;
        private readonly ITravianUserRepository _travianUserRepository;
        private readonly ISchedulerService _schedulerService;
        private readonly IJobInfoProvider _jobInfoProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobManagerController> _logger;

        public JobManagerController(
            UserManager<BotUser> userManager, 
            ITravianUserRepository travianUserRepository, 
            ISchedulerService schedulerService,
            IJobInfoProvider jobInfoProvider,
            IServiceProvider serviceProvider,
            ILogger<JobManagerController> logger)
        {
            _userManager = userManager;
            _travianUserRepository = travianUserRepository;
            _schedulerService = schedulerService;
            _jobInfoProvider = jobInfoProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string botUserName, string travianUserName)
        {
            ViewData["player"] = travianUserName;
            if (TempData["Error"] != null)
            {
                ViewBag.ErrorMessage = TempData["Error"];
            }

            IEnumerable<JobInfoViewModel> model = null;
            try
            {
                var player = await _travianUserRepository.GetUserByName(travianUserName, botUserName);
                model = await _jobInfoProvider.GetJobsDetailsForPlayer(player);
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItem, exc, exc.Message);
                ViewBag.ErrorMessage = "Unable to provide jobs information";
            }

            return View(model.OrderBy(x => x.NextExecutionTime));
        }

        public async Task<IActionResult> CancelJob(string name, string group)
        {
            try
            {
                var result = await _schedulerService.InterruptCommandAsync(new JobKey(name, group));
                if (!result)
                {
                    TempData["Error"] = $"Unable to cancel job {name} from {group}";
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.BackgroundJobInterruptionException, exc, exc.Message);

                TempData["Error"] = exc.Message;
            }

            return RedirectToAction("Index");
        }
    }
}