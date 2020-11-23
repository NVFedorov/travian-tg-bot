using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Scheduler;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.ViewModels;
using TTB.Common.Extensions;
using TTB.DAL.Models;

namespace TravianTelegramBot.Services.Impl
{
    public class JobInfoProvider : IJobInfoProvider
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<JobInfoProvider> _logger;

        public JobInfoProvider(IScheduler scheduler, ILogger<JobInfoProvider> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task<IEnumerable<JobInfoViewModel>> GetJobsDetailsForPlayer(TravianUser player)
        {
            var groupName = SchedulerService.BuildGroupKey(player.UserName, player.BotUserName);
            var matcher = GroupMatcher<JobKey>.GroupContains(groupName);
            var result = new List<JobInfoViewModel>();
            foreach (var key in await _scheduler.GetJobKeys(matcher))
            {
                var triggers = await _scheduler.GetTriggersOfJob(key);
                var detail = await _scheduler.GetJobDetail(key);
                var nextTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc();
                if (!nextTime.HasValue)
                {
                    var cmd = detail.JobDataMap[AbstractJob.JobExecutionDataKey] as IQueueableCommand;
                    nextTime = cmd.Start;
                }

                result.Add(new JobInfoViewModel
                {
                    Name = key.Name,
                    Group = key.Group,
                    NextExecutionTime = nextTime.Value.ToDisplayStringApplyTimeZone(player.PlayerData.TimeZone)
                });
            }

            return result;
        }
    }
}
