using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using TravianTelegramBot.Client;
using TravianTelegramBot.Services;
using TTB.Common.Settings;
using TTB.DAL.Models;

namespace TravianTelegramBot.Controllers
{
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<BotController> _logger;

        private const string ExceptionMessage = "Unable to read response from WEB API service.";

        public BotController(IMessageService messageService, ILogger<BotController> logger)
        {
            this._messageService = messageService;
            this._logger = logger;
        }
        
        [HttpPost]
        [Route("post")]
        public IActionResult Post([FromBody]Update update)
        {
            try
            {
                _messageService.ProcessMessage(update);
            }
            catch(Exception exc)
            {
                this._logger.LogError(LoggingEvents.TelegramClientException, exc, "Unable to process incoming message from telegram");
            }

            return Ok();
        }
    }
}