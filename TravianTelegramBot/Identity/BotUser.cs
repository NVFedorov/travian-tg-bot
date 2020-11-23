using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.Identity
{
    [CollectionName("botUsers")]
    public class BotUser : MongoIdentityUser<string>
    {
        public long ChatId { get; set; }
        public bool? IsEnabled { get; set; }
    }
}
