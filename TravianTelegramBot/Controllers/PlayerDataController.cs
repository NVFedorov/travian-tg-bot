using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Services;
using TTB.Common.Settings;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize]
    public class PlayerDataController : Controller
    {
        private readonly ITravianUserRepository _travianUserRepository;
        private readonly ILogger<PlayerDataController> _logger;
        private readonly ICommandFactory _commandFactory;
        private readonly UserManager<BotUser> _userManager;

        public PlayerDataController(
            ITravianUserRepository travianUserRepository,
            ICommandFactory commandFactory,
            UserManager<BotUser> userManager,
            ILogger<PlayerDataController> logger)
        {
            _travianUserRepository = travianUserRepository;
            _logger = logger;
            _commandFactory = commandFactory;
            _userManager = userManager;
        }

        [Route("[controller]")]
        public async Task<IActionResult> Index(string userName)
        {
            var msg = TempData["ErrorMessage"]?.ToString() ?? string.Empty;
            try
            {
                var travianUser = await _travianUserRepository.GetUserByName(userName, User.Identity.Name);
                if (travianUser != null)
                {
                    if (travianUser.PlayerData == null)
                    {
                        travianUser.PlayerData = new PlayerDataModel
                        {
                            UserName = userName
                        };
                    }
                    else if (travianUser.PlayerData.UserName != userName)
                    {
                        travianUser.PlayerData.UserName = userName;
                    }

                    ViewBag.IsActive = travianUser.IsActive;
                    return View(travianUser.PlayerData);
                }
                else
                {
                    msg = "Unable to find player data.";
                    ViewBag.ErrorMessage = msg;
                    _logger.LogError(LoggingEvents.GetItemNotFound, msg);
                }
            }
            catch (Exception exc)
            {
                msg = "Unable to find player data.";
                ViewBag.ErrorMessage = msg;
                _logger.LogError(LoggingEvents.UpdateItemException, exc, exc.Message + $". Unable to find player data. Travian User Name: [{userName}].");
            }

            return View();
        }

        public async Task<IActionResult> Update(PlayerDataModel model)
        {
            var travianUser = await _travianUserRepository.GetUserByName(model.UserName, User.Identity.Name);
            travianUser.PlayerData = model;
            await _travianUserRepository.ReplacePlayerData(travianUser);
            return RedirectToAction("Index", new { userName = travianUser.UserName });
        }

        public async Task<IActionResult> RunUpdate(string userName)
        {
            var message = string.Empty;
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var cmd = _commandFactory.GetCommand(nameof(UpdateUserInfoCommand), user.ChatId);
                if (cmd != null)
                {
                    await cmd.Execute();
                    return Ok(new { message = "Player Data updated successfully" });
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);
                message = exc.Message;
            }

            return BadRequest(new { message = "Unable to update Player Data" + (string.IsNullOrEmpty(message) ? $": {message}" : ".") });
        }
    }
}