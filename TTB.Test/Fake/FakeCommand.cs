using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TravianTelegramBot.Commands;

namespace TTB.Test.Fake
{
    public class FakeCommand : IQueueableCommand
    {
        public string Name { get; }
        public Type JobType { get; }
        public DateTimeOffset Start { get; set; }
        public string Cron { get; }

        public async Task Execute(string parameters = "")
        {
        }
    }
}
