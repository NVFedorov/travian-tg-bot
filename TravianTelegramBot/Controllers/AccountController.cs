using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravianTelegramBot.Extensions;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Settings;
using TravianTelegramBot.ViewModels;
using TTB.Common.Settings;

namespace TravianTelegramBot.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<BotUser> _userManager;
        private readonly SignInManager<BotUser> _signInManager;
        private readonly RoleManager<BotUserRole> _roleManager;
        private readonly ISecretProvider _secretProvider;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<BotUser> userManager, 
            SignInManager<BotUser> signInManager, 
            RoleManager<BotUserRole> roleManager,
            ILoggerFactory loggerFactory,
            ISecretProvider secretProvider)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._secretProvider = secretProvider;
            this._logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    var role = new BotUserRole("Admin");
                    await _roleManager.CreateAsync(role);
                }
                if (!await _roleManager.RoleExistsAsync("LogViewer"))
                {
                    var role = new BotUserRole("LogViewer");
                    await _roleManager.CreateAsync(role);
                }
            }
            catch(Exception exc)
            {
                this._logger.LogError(LoggingEvents.IdentityOperationException, exc, "Unable to create roles");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogDebug(LoggingEvents.UserSignedIn, $"User [{User.Identity.Name}] logged in.");
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(LoggingEvents.UserIsLockedOut, "User account locked out.");
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register([FromQuery]string token)
        {
            ViewData["Token"] = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var secret = await this._secretProvider.CheckSecretCode(model.SecretCode);
                if (secret != null)
                {
                    try
                    {
                        var user = new BotUser { UserName = model.UserName, ChatId = Convert.ToInt64(model.Token), IsEnabled = true };
                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            if (secret.AdminPrivileges)
                            {
                                if (!await _roleManager.RoleExistsAsync("Admin"))
                                {
                                    var role = new BotUserRole("Admin");
                                    await _roleManager.CreateAsync(role);
                                }

                                var assignResult = await _userManager.AddToRoleAsync(user, "Admin");
                                if (!assignResult.Succeeded)
                                {
                                    var msg = string.Empty;
                                    foreach (var error in assignResult.Errors)
                                    {
                                        msg += $"Code[{error.Code}]: {error.Description}";
                                    }

                                    _logger.LogError($"Unable to assign user to admin role: \r\n {msg}");
                                }
                            }

                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation(3, "User created a new account with password.");
                            return RedirectToAction("Index", "Home");
                        }
                        AddErrors(result);
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError(LoggingEvents.RegisterAttemptFailed, exc, "Unable to register new user.");
                        ModelState.AddModelError(string.Empty, "Unable to register new user.");
                    }
                }
                else
                {
                    _logger.LogWarning(LoggingEvents.RegisterAttemptFailedWithWrongSecurityCode, $"Somebody tried to register with wrong secret code. Register parameters: " +
                        $"user name [{model.UserName}]; token: [{model.Token}]; secret code: [{model.SecretCode}]");
                    ModelState.AddModelError("SecretCode", "Unrecognized secret code provided");
                }
            }

            ViewData["Token"] = model.Token;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            var name = User.Identity.Name;
            await _signInManager.SignOutAsync();
            _logger.LogDebug(LoggingEvents.UserSignedOut, $"User [{name}] logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}