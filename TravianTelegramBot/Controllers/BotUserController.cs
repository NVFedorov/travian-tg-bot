using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Extensions;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Settings;
using TTB.Common.Settings;

namespace TravianTelegramBot.Controllers
{
    public class BotUserController : Controller
    {
        private readonly UserManager<BotUser> userManager;
        private readonly RoleManager<BotUserRole> roleManager;
        private readonly ILogger<BotUserController> logger;

        public BotUserController(
            UserManager<BotUser> userManager,
            RoleManager<BotUserRole> roleManager,
            ILogger<BotUserController> logger)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        public IActionResult List()
        {
            List<BotUser> model = null;
            try
            {
                model = this.userManager.Users.ToList();
            }
            catch (Exception exc)
            {
                this.logger.LogError(LoggingEvents.IdentityOperationException, exc, "Unable to retrive users from Databse.");
            }

            return View(model);
        }

        public async Task<IActionResult> Enable(string userName)
        {
            try
            {
                var user = await userManager.FindByNameAsync(userName);
                user.IsEnabled = true;
                await userManager.UpdateAsync(user);
            }
            catch (Exception exc)
            {
                this.logger.LogError(LoggingEvents.GetItemNotFound, exc, "Unable to find user by name [{userName}]", userName);
            }

            return RedirectToAction("List");
        }

        public async Task<IActionResult> Block(string userName)
        {
            try
            {
                var user = await userManager.FindByNameAsync(userName);
                user.IsEnabled = false;
                await userManager.UpdateAsync(user);
            }
            catch (Exception exc)
            {
                this.logger.LogError(LoggingEvents.GetItemNotFound, exc, "Unable to find user by name [{userName}]", userName);
            }

            return RedirectToAction("List");
        }

        public async Task<IActionResult> UpdateRole(bool assign, string userName, string role = "LogViewer")
        {
            try
            {
                var user = await userManager.FindByNameAsync(userName);
                var isInRole = await this.userManager.IsInRoleAsync(user, role);
                if (assign)
                {
                    if (!isInRole)
                    {
                        await this.userManager.AddToRoleAsync(user, role);
                    }
                }
                else
                {
                    if (isInRole)
                    {
                        await this.userManager.RemoveFromRoleAsync(user, role);
                    }
                }
            }
            catch (Exception exc)
            {
                this.logger.LogError(LoggingEvents.GetItemNotFound, exc, "Unable to find user by name [{userName}]", userName);
                return BadRequest("Operation failed.");
            }

            return Ok("Success");
        }
    }
}