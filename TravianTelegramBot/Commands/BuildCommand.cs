using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravianTelegramBot.Scheduler.Jobs;

namespace TravianTelegramBot.Commands
{
    public class BuildCommand : AbstractQueueableCommand
    {
        public BuildCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
        }

        public override Type JobType => typeof(BuildingPlanExecutionJob);
        public override DateTimeOffset Start { get; set; }
        public override string Name => nameof(BuildCommand);
    }
}
