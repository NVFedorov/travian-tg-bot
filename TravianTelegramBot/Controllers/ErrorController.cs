using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Extensions;
using TravianTelegramBot.Settings;
using TravianTelegramBot.ViewModels;
using TTB.Common.Settings;

namespace TravianTelegramBot.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Details()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddhhmmssttt");
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();            
            this.logger.LogCritical(LoggingEvents.CriticalError, exceptionHandlerPathFeature.Error, "Unhadeled error occurred. Request ID: [{requestId}]", requestId);

            return View(new ErrorViewModel
            {
                RequestId = requestId,
                Message = "Unexpected error occurred.",
                Timestamp = timestamp
            });
        }
    }
}