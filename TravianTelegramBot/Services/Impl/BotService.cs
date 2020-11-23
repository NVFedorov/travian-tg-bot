using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TravianTelegramBot.Settings;
using TTB.Common.Settings;

namespace TravianTelegramBot.Services.Impl
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;
        private readonly ILogger<BotService> _logger;

        public BotService(IOptions<BotConfiguration> config, ILogger<BotService> logger)
        {
            _config = config.Value;
            // use proxy if configured in appsettings.*.json
            Client = string.IsNullOrEmpty(_config.Socks5Host)
                ? new TelegramBotClient(_config.BotToken)
                : new TelegramBotClient(
                    _config.BotToken,
                    new HttpToSocks5Proxy(_config.Socks5Host, _config.Socks5Port));
            _logger = logger;
        }

        public TelegramBotClient Client { get; }

        public async Task<Message> SendTextMessageAsync(long chatId, string message)
        {
            try
            {
                return await Client.SendTextMessageAsync(chatId, message);
            }
            catch(ChatNotFoundException exc)
            {
                _logger.LogError(LoggingEvents.TelegramClientException, exc, $"Unable to find chat with id: {chatId}");
            }
            catch(Exception exc)
            {
                _logger.LogError(LoggingEvents.TelegramClientException, exc, exc.Message);
            }

            return null;
        }
    }
}
