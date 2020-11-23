using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Services;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Commands
{
    public abstract class AbstractCommand
    {
        protected readonly IBotService _bot;
        protected readonly BotUser _botUser;
        protected readonly ILogger<AbstractCommand> _logger;
        protected readonly long _chatId;

        protected TravianUser _travianUser;
        private readonly ITravianUserRepository _travianUserRepo;
        private readonly HttpContext _httpContext;

        public AbstractCommand(IServiceProvider service, long chatId)
        {
            this._bot = service.GetService<IBotService>();
            var userManager = service.GetService<IBotUserProvider>();
            this._botUser = userManager.FindByChatId(chatId);
            this._httpContext = service.GetService<IHttpContextAccessor>()?.HttpContext;
            this._travianUserRepo = service.GetService<ITravianUserRepository>();
            this._logger = service.GetService<ILogger<AbstractCommand>>();
            this._chatId = chatId;
        }

        public abstract string Name { get; }
        public string Action { get; set; }

        public virtual async Task Execute(string parameters = "")
        {
            if (this._botUser == null)
            {
                var loginUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{_httpContext.Request.PathBase}/Account/Register?token={this._chatId}";
                await _bot.SendTextMessageAsync(this._chatId, $"You are not authorized to use the bot. Please follow this link to login: {loginUrl}");
            }
            else
            {
                if (await UpdateTravianUser())
                {
                    try
                    {
                        await this.ExecuteCommand(parameters);
                    }
                    catch (Exception exc)
                    {
                        await _bot.SendTextMessageAsync(
                                this._chatId,
                                $"Unable to execute command. Error Id: {LoggingEvents.CommandException}");
                        this._logger.LogError(LoggingEvents.CommandException, exc, exc.Message);
                    }
                }
            }
        }

        protected abstract Task ExecuteCommand(string parameters = "");
        
        private async Task<bool> UpdateTravianUser()
        {
            var result = false;
            if (this._travianUser == null)
            {
                try
                {
                    this._travianUser = await this._travianUserRepo.GetActiveUser(this._botUser.UserName);
                    if (this._travianUser == null)
                    {
                        await _bot.SendTextMessageAsync(this._chatId, $"Unable to find active user to start watching. Please configure the travian user first. Error Id: {10}");
                    }
                    else
                    {
                        result = true;
                    }
                }
                catch (Exception exc)
                {
                    await _bot.SendTextMessageAsync(
                            this._chatId,
                            $"Unable to find active user to start watching. Please configure the travian user first. Error Id: {10}");
                    this._logger.LogError(10, $"{exc.Message}: \r\n {exc.StackTrace}");
                }
            }
            else
            {
                result = true;
            }

            return result;
        }
    }
}
