using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTB.DAL.Models
{
    public class BackgroundJobModel
    {
        public string BackgroundJobId { get; set; }
        public string BotUserName { get; set; }
        public bool IsRunning { get; set; }
        public IEnumerable<string> CommandsQueue { get; set; }
    }
}
