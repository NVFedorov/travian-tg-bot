using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.WebApiResponse
{
    public class WatchResponse : Response
    {
        [JsonProperty("userUnderAttack")]
        public bool IsUserUnderAttack;
    }
}
