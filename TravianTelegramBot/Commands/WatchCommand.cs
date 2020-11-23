using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Settings.CommandSettings;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Commands
{
    public class WatchCommand : AbstractQueueableCommand
    {
        private WatchCommandConfiguration _config;

        public WatchCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            var options = service.GetService<IOptionsSnapshot<WatchCommandConfiguration>>();
            _config = options?.Value;
        }

        public override string Name => nameof(WatchCommand);
        public override Type JobType => typeof(ObserverJob);

        // every 10 minutes
        // TODO: change with value from configs
        public override string Cron => string.IsNullOrEmpty(_config?.Cron) ? "0 0/10 * 1/1 * ? *" : _config.Cron;
    }
}
