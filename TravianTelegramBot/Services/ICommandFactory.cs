using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TravianTelegramBot.Commands;

namespace TravianTelegramBot.Services
{
    public interface ICommandFactory
    {
        AbstractCommand GetCommand(Message message);
        AbstractCommand GetCommand(string commandName, long chatId);
        IQueueableCommand GetQueueableCommand(string commandName, long chatId);
    }
}
