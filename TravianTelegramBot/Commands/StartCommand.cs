using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TravianTelegramBot.Settings;
using TTB.Common.Settings;

namespace TravianTelegramBot.Commands
{
    public class StartCommand : AbstractCommand
    {

        public StartCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
        }

        public override string Name => nameof(StartCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            try
            {
                await _bot.Client.SendTextMessageAsync(this._chatId, "Hello from Travian Bot!");
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.TelegramClientException, exc, $"Unable to send message to chat [{this._chatId}]");
            }
        }
    }
}
