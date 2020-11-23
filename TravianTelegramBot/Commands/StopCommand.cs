using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Scheduler;

namespace TravianTelegramBot.Commands
{
    public class StopCommand : AbstractCommand
    {
        private readonly ISchedulerService scheduler;

        public StopCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            this.scheduler = service.GetService<ISchedulerService>();
        }

        public override string Name => nameof(StopCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            this._logger.LogDebug("Pausing all the jobs");
            await this.scheduler.PauseAll();
            this._logger.LogDebug("All the jobs have been paused.");
        }
    }
}
