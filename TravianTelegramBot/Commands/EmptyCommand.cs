using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using TravianTelegramBot.Scheduler.Jobs;

namespace TravianTelegramBot.Commands
{
    public class EmptyCommand
    {
        private readonly IScheduler _scheduler;

        public EmptyCommand(IServiceProvider service, long chatId)
        {
            _scheduler = service.GetService<IScheduler>();
        }

        public Type JobType => typeof(EmptyJob);
        public string Name => nameof(EmptyCommand);
        public string Cron => "0 0/1 * 1/1 * ? *";

        public async Task Execute(string parameters = "")
        {
            var jobData = new JobExecutionData
            {
                Cron = Cron,
                JobType = JobType
            };

            IJobDetail job = JobBuilder.Create(typeof(EmptyJob))
                    .WithIdentity($"{Name}_{JobType}")
                    .Build();

            job.JobDataMap[AbstractJob.JobExecutionDataKey] = jobData;

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobData.JobType.Name}:trigger", $"{jobData.JobType.Name}")
                .WithCronSchedule(jobData.Cron).Build();

            await _scheduler.ScheduleJob(job, trigger, new System.Threading.CancellationToken());
        }
    }
}
