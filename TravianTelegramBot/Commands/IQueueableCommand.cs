using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TravianTelegramBot.Commands
{
    public interface IQueueableCommand
    {
        Task Execute(string parameters = "");
        string Name { get; }
        Type JobType { get; }
        DateTimeOffset Start { get; set; }
        string Cron { get; }
    }
}
