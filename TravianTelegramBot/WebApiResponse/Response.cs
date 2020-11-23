using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.WebApiResponse
{
    public enum BotStatus
    {
        Default,
        Running,
        Finished,
        Stopped,
        Error
    }

    public class Response
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("status")]
        public BotStatus Status { get; set; }
    }
}
