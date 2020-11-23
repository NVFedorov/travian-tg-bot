using System;
using TravianTelegramBot.Scheduler.Jobs;

namespace TravianTelegramBot.Commands
{
    public class PrepareToAttackCommand : AbstractQueueableCommand
    {        
        public PrepareToAttackCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
        }

        public override Type JobType => typeof(PrepareToAttackJob);

        public override DateTimeOffset Start { get; set; }

        public override string Name => nameof(PrepareToAttackCommand);
    }
}
