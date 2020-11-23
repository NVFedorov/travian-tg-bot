using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize(Roles = "Admin, LogViewer")]
    public class LogController : Controller
    {
        private readonly ILogRepository<ManagerLogEntryModel> _managerLogRepo;
        private readonly ILogRepository<LogEntryModel> _webapiLogRepo;
        private readonly ILogger<LogController> _logger;

        public LogController(ILogRepository<ManagerLogEntryModel> managerLogRepo, ILogRepository<LogEntryModel> webapiLogRepo, ILogger<LogController> logger)
        {
            _managerLogRepo = managerLogRepo;
            _webapiLogRepo = webapiLogRepo;
            _logger = logger;
        }

        public async Task<IActionResult> ManagerLogs(string level)
        {
            ViewData["Selected"] = level;
            List<ManagerLogEntryModel> model = null;
            try
            {
                if (!string.IsNullOrEmpty(level))
                {
                    model = await _managerLogRepo.GetLast(100, level);
                }
                else
                {
                    model = await _managerLogRepo.GetLast(100);
                }

                model.ForEach(x =>
                {
                    x.Exception?.Replace("\r\n", "<br />\r\n");
                    x.RenderedMessage?.Replace("\r\n", "<br />\r\n");
                });
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to retrive manager logs from database with level [{level}]", level);
                return BadRequest(new
                {
                    result = $"Unable to retrive logs with level [{level}]"
                });
            }

            return View("~/Views/LogViews/List.cshtml", model);
        }

        public async Task<IActionResult> GetLogs(string level = "")
        {
            List<LogEntryModel> model = null;
            try
            {
                model = (await _managerLogRepo.GetLast(50, level)).Cast<LogEntryModel>().ToList();
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to retrive logs with level [{level}]");
                return BadRequest(new
                {
                    result = $"Unable to retrive logs with level [{level}]"
                });
            }

            return Ok(model);
        }
    }
}