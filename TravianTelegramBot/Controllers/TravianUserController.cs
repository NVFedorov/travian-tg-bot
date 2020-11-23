using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Scheduler;

using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Controllers
{
    [Authorize]
    public class TravianUserController : Controller
    {
        private readonly ITravianUserRepository _repo;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISchedulerService _scheduler;
        private readonly UserManager<BotUser> _userManager;
        private readonly IGameplayClient _client;
        private readonly ILogger<TravianUserController> _logger;

        public TravianUserController(
            ITravianUserRepository repo,
            IServiceProvider serviceProvider,
            ISchedulerService scheduler,
            UserManager<BotUser> userManager,
            IGameplayClient client,
            ILogger<TravianUserController> logger)
        {
            this._repo = repo;
            this._serviceProvider = serviceProvider;
            _scheduler = scheduler;
            this._userManager = userManager;
            _client = client;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Cookie(string userName)
        {
            try
            {
                var model = await _repo.GetUserByName(userName, User.Identity.Name);
                ViewData["UserName"] = userName;
                return View(model.Cookies);
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.GetItemNotFound, exc, "Can not find cookies for user [{userName}]", userName);
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cookie([FromQuery]string userName, [FromBody]CookiesViewModel cookies)
        {
            if (cookies == null || cookies.Cookies == null)
            {
                return BadRequest(new
                {
                    result = "Cookies not found"
                });
            }
            try
            {
                var user = await _repo.GetUserByName(userName, User.Identity.Name);
                user.Cookies = cookies.Cookies;
                await _repo.Update(user);
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.UpdateItemNotFound, exc, $"Unable to update cookies for travian user [{userName}] of bot user [{User.Identity.Name}]");
                return BadRequest(new
                {
                    result = "Unable to update cookies"
                });
            }

            return Ok(new
            {
                result = "success"
            });
        }

        public async Task<IActionResult> Delete(string userName)
        {
            try
            {
                var current = await _repo.GetUserByName(userName, User.Identity.Name);
                await _scheduler.InterruptAll();
                await _client.Logout(current);
                await _repo.Delete(userName, User.Identity.Name);
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.DeleteItemException, exc, $"Unable to delete travian user [{userName}] of bot user [{User.Identity.Name}]");
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TravianUser model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _repo.GetUserByName(model.UserName, User.Identity.Name);
                    if (existing != null)
                    {
                        ModelState.AddModelError("UserName", "Player with this name already exists");
                        return View(model);
                    }
                }
                catch (Exception exc)
                {
                    this._logger.LogError(LoggingEvents.GetItemException, exc, $"Unable to update travian user [{model.UserName}] of bot user [{User.Identity.Name}]");
                }

                if (model.IsActive)
                {
                    var existingActive = await _repo.GetActiveUser(User.Identity.Name);
                    if (existingActive != null)
                    {
                        existingActive.IsActive = false;
                        try
                        {
                            await _client.Logout(existingActive);
                            await _repo.Update(existingActive);
                        }
                        catch (Exception exc)
                        {
                            this._logger.LogError(LoggingEvents.UpdateItemException, exc, $"Unable to update travian user [{model.UserName}] of bot user [{User.Identity.Name}]");
                        }
                    }
                }

                try
                {
                    await _repo.Insert(model);
                }
                catch (Exception exc)
                {
                    this._logger.LogError(LoggingEvents.InsertItemException, exc, $"Unable to update travian user [{model.UserName}] of bot user [{User.Identity.Name}]");
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userName)
        {
            try
            {
                var model = await _repo.GetUserByName(userName, User.Identity.Name);
                if (model.BotUserName == User.Identity.Name)
                {
                    return View(model);
                }
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to find travian user [{userName}] of bot user [{User.Identity.Name}]");
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TravianUser model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repo.Update(model);
                }
                catch (Exception exc)
                {
                    this._logger.LogError(LoggingEvents.UpdateItemException, exc, $"Unable to update travian user [{model.UserName}] of bot user [{User.Identity.Name}]");
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        public async Task<IActionResult> List()
        {
            IEnumerable<TravianUser> travianUsers = null;
            try
            {
                travianUsers = await _repo.GetUsersByBotUser(User.Identity.Name);
                //var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
                //var emptyCmd = new EmptyCommand(_serviceProvider, user.ChatId);
                //await emptyCmd.Execute();
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.GetItemNotFound, exc, $"Unable to find travian users of bot user [{User.Identity.Name}]");
            }

            return View(travianUsers);
        }

        public async Task<IActionResult> SetActive(string userName, bool active)
        {
            try
            {
                var currentActive = await _repo.GetActiveUser(User.Identity.Name);
                if (currentActive != null)
                {
                    await _client.Logout(currentActive);
                    if (active)
                    {
                        var travianUsers = await _repo.GetUsersByBotUser(User.Identity.Name);
                        travianUsers.ToList().ForEach(item => item.IsActive = false);
                        await _repo.Update(travianUsers);
                    }
                }

                var current = await _repo.GetUserByName(userName, User.Identity.Name);
                current.IsActive = active;                
                await _repo.Update(current);
                await _scheduler.InterruptAll();
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.UpdateItemException, exc, $"Unable to update active travian user [{userName}] of bot user [{User.Identity.Name}] with IsActive = [{active}]");
                return BadRequest("Unable to update user");
            }

            return Ok("success");
        }

        public async Task<IActionResult> Watch(string userName)
        {
            var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
            var watchCmd = new WatchCommand(this._serviceProvider, user.ChatId);
            watchCmd.Execute().Wait();

            //var watchCmd2 = new PrepareToAttackCommand(this.serviceProvider, user.ChatId);
            //watchCmd2.Start = DateTimeOffset.Now.AddMinutes(1);
            //Task.WhenAll(watchCmd.Execute(), watchCmd2.Execute());

            //await Task.Delay(10000);
            //var statusCmd = new StatusCommand(this.serviceProvider, user.ChatId);
            //statusCmd.Execute();

            //await Task.Run(() =>
            //{
            //    watchCmd.Execute();
            //    watchCmd2.Execute();
            //});

            return RedirectToAction("List");
            //WatchResponse response;
            //try
            //{
            //    response = await client.Watch(userName);
            //    if (response.IsUserUnderAttack)
            //    {
            //        try
            //        {
            //            var notifications = await notificationRepo.GetIncomingAttacksAndScans(userName);
            //            return View("NotificationsList", notifications);
            //        }
            //        catch (Exception exc)
            //        {
            //            this.logger.LogError(LoggingEvents.DbOpertationException, exc, "Unable to complete request to WEB API service. Method: Watch.");
            //        }
            //    }
            //}
            //catch (Exception exc)
            //{
            //    response = new WatchResponse
            //    {
            //        Error = "Something went wrong with completing the Watch request to WEB API service.",
            //        Status = BotStatus.Error
            //    };

            //    this.logger.LogError(LoggingEvents.WebApiException, exc, "Unable to complete request to WEB API service. Method: Watch.");
            //}

            //return View(response);
        }

        public async Task<IActionResult> Pause()
        {
            var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
            var stopCmd = new StopCommand(this._serviceProvider, user.ChatId);
            await stopCmd.Execute();

            return RedirectToAction("List");            
        }

        public async Task<IActionResult> Cancel()
        {
            var user = await this._userManager.FindByNameAsync(this.User.Identity.Name);
            var watchCmd = new WatchCommand(this._serviceProvider, user.ChatId);
            await watchCmd.Execute("stop");

            return RedirectToAction("List");
        }
    }
}