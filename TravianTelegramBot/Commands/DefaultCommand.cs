using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TravianTelegramBot.Services;
using TravianTelegramBot.Settings;
using TTB.Common.Settings;

namespace TravianTelegramBot.Commands
{
    public class DefaultCommand : AbstractCommand
    {
        public DefaultCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
        }

        public override string Name => nameof(DefaultCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            try
            {
                await _bot.Client.SendTextMessageAsync(this._chatId, "Sorry, the command was not recognized!");
            }
            catch(Exception exc)
            {
                this._logger.LogError(LoggingEvents.TelegramClientException, exc, $"Unable to send message to chat [{this._chatId}]");
            }
        }
    }
}
