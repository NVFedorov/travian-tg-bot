using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TTB.Common.Extensions;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class EmptyJob : IJob
    {
        public const string Cron = "0 0/1 * 1/1 * ? *";

        private readonly IServiceProvider _service;

        public EmptyJob(IServiceProvider service)
        {
            _service = service;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _service.CreateScope())
            {
                await Task.Delay(100);
                var logger = scope.ServiceProvider.GetService<ILogger<EmptyJob>>();
                logger.LogDebug($"Empty Job executed at {DateTimeOffset.Now.ToFormattedString()}");
            }
        }
    }
}
