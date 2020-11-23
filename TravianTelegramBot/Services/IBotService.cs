using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TravianTelegramBot.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        Task<Message> SendTextMessageAsync(long chatId, string message);
    }
}
