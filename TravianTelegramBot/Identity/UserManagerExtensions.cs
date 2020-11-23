using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.Identity
{
    public static class UserManagerExtensions
    {
        public static BotUser FindByNameAsync(this UserManager<BotUser> um, string name)
        {
            return um?.Users?.SingleOrDefault(x => x.UserName == name);
        }

        public static BotUser FindByChatId(this UserManager<BotUser> um, long chatId)
        {
            return um?.Users?.SingleOrDefault(x => x.ChatId == chatId);
        }
    }
}
