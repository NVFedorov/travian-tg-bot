using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class JobExecutionData
    {
        public TravianUser TravianUser { get; set; }
        public DateTimeOffset Start { get; set; }
        public string Cron { get; set; }
        public Type JobType { get; set; }
    }
}
