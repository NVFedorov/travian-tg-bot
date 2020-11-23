using AspNetCore.Identity.MongoDbCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.Identity
{
    public class BotUserRole : MongoIdentityRole<string>
    {
        public BotUserRole() : base()
        {
        }

        public BotUserRole(string roleName) : base(roleName)
        {
        }
    }
}
