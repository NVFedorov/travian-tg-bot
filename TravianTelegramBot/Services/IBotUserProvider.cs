using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravianTelegramBot.Identity;

namespace TravianTelegramBot.Services
{
    public interface IBotUserProvider
    {
        BotUser FindByChatId(long chatId);
        Task<BotUser> FindByNameAsync(string userName);
    }

    public class BotUserProvider : IBotUserProvider
    {
        private readonly UserManager<BotUser> userManager;

        public BotUserProvider(UserManager<BotUser> userManager)
        {
            this.userManager = userManager;
        }

        public BotUser FindByChatId(long chatId)
        {
            return this.userManager?.Users?.SingleOrDefault(x => x.ChatId == chatId);
        }

        public async Task<BotUser> FindByNameAsync(string userName)
        {
            return await this.userManager?.FindByNameAsync(userName);
        }
    }

}
