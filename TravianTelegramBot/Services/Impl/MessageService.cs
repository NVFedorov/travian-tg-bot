using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TravianTelegramBot.Extensions;
using TTB.Common.Settings;

namespace TravianTelegramBot.Services.Impl
{
    public class MessageService : IMessageService
    {
        private readonly IBotService _bot;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IBotService bot,
            ICommandFactory commandFactory,
            ILogger<MessageService> logger)
        {
            this._bot = bot;
            this._commandFactory = commandFactory;
            this._logger = logger;
        }

        public async Task ProcessMessage(Update update)
        {
            if (update.Type != UpdateType.Message)
            {
                return;
            }

            var message = update.Message;

            _logger.LogInformation("Received Message from {0}", message.Chat.Id);

            var command = _commandFactory.GetCommand(message);
            try
            {
                //await _bot.Client.SendTextMessageAsync(message.Chat.Id, $"Starting the {command.Name}");
                await command.Execute(update.Message.Text);
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.TelegramClientException, exc, exc.Message);
                await _bot.Client.SendTextMessageAsync(message.Chat.Id, "Something went wrong");
            }

            //if (message.Type == MessageType.Text)
            //{
            //    // Echo each Message
            //    await _bot.Client.SendTextMessageAsync(message.Chat.Id, message.Text);
            //}
        }

        public Task SendMessage(string text)
        {
            throw new NotImplementedException();
        }
    }
}
