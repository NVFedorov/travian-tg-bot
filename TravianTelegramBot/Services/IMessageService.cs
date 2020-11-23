using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TravianTelegramBot.Services
{
    public interface IMessageService
    {
        Task SendMessage(string text);
        Task ProcessMessage(Update update);
    }
}
