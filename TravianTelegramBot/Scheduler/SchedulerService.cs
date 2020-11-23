using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Extensions;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Settings;
using TTB.Common.Extensions;
using TTB.Common.Settings;
using TTB.DAL.Models;

namespace TravianTelegramBot.Scheduler
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<SchedulerService> _logger;

        public SchedulerService(IScheduler scheduler, ILogger<SchedulerService> logger)
        {
            this._scheduler = scheduler;
            this._logger = logger;
        }

        public async Task<bool> IsJobRunning(JobKey key)
        {
            var running = await this._scheduler.GetJobDetail(key);
            return running != null;
        }

        public async Task<bool> InterruptCommandAsync(JobKey key)
        {
            try
            {
                await this._scheduler.Interrupt(key);
                await this._scheduler.DeleteJob(key);
                return true;
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.BackgroundJobExecutingException, exc, $"Unable to interrut the Job by job key: [{key.Name + key.Group}]");
                return false;
            }
        }

        public async Task<JobKey> ScheduleCommandAsync(JobExecutionData jobData, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jobData.JobType.Name))
                {
                    throw new ArgumentNullException(nameof(jobData.JobType.Name), "The command name must be specified.");
                }

                IJobDetail job = JobBuilder.Create(typeof(MasterJob))
                    .WithIdentity(BuildJobKey(jobData))
                    .Build();

                job.JobDataMap[AbstractJob.JobExecutionDataKey] = jobData;

                var builder = TriggerBuilder.Create()
                    .WithIdentity($"{jobData.JobType.Name}:trigger:for:{jobData.TravianUser.UserName}:timestamp:{jobData.Start.ToTimeStamp()}", $"{jobData.TravianUser.BotUserName}.group")
                    .WithDescription($"{jobData.JobType}.trigger.With: CRON: {jobData.Cron}, StartDateTime: {jobData.Start.ToFormattedStringWithMills()}")
                    .StartAt(jobData.Start);

                var trigger = string.IsNullOrEmpty(jobData.Cron)
                    ? builder.Build()
                    : builder.WithCronSchedule(jobData.Cron).Build();

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
                if (!string.IsNullOrEmpty(jobData.Cron) || jobData.Start < DateTimeOffset.UtcNow && (DateTimeOffset.UtcNow - jobData.Start).TotalMinutes < 5)
                {
                    await _scheduler.TriggerJob(job.Key);
                }

                return job.Key;
            }
            catch (Exception exc)
            {
                this._logger.LogError(LoggingEvents.BackgroundJobCreationException, exc, $"Unable to create new Job instance of type [{jobData.JobType}]");
                return null;
            }
        }

        public static JobKey BuildJobKey(JobExecutionData jobData)
        {
            var additional = string.Empty;
            if (string.IsNullOrEmpty(jobData.Cron))
            {
                additional = $":timestamp:{jobData.Start.ToTimeStamp()}";
            }

            return new JobKey($"{jobData.JobType.Name}:job:for:{jobData.TravianUser.UserName}{additional}", BuildGroupKey(jobData.TravianUser.UserName, jobData.TravianUser.BotUserName));
        }

        public static string BuildGroupKey(string playerName, string botUserName)
        {
            return $"{playerName}:player:{botUserName}:group";
        }

        public async Task<Dictionary<IJobDetail, DateTimeOffset>> GetRunningJobsList()
        {            
            var result = new Dictionary<IJobDetail, DateTimeOffset>();
            var groups = await _scheduler.GetJobGroupNames();
            foreach (var group in groups)
            {
                var matcher = GroupMatcher<JobKey>.GroupContains(group);
                foreach (var key in await _scheduler.GetJobKeys(matcher))
                {
                    var triggers = await _scheduler.GetTriggersOfJob(key);
                    var detail = await _scheduler.GetJobDetail(key);
                    var nextTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc();
                    if (!nextTime.HasValue)
                    {
                        var cmd = detail.JobDataMap[AbstractJob.JobExecutionDataKey] as IQueueableCommand;
                        nextTime = cmd?.Start;
                    }

                    result.Add(detail, nextTime ?? DateTimeOffset.MinValue);
                }
            }

            return result;
        }

        public async Task PauseAll()
        {
            await _scheduler.PauseAll();
        }

        public async Task<IEnumerable<ITrigger>> GetJobTrigger(JobKey key)
        {
            var triggers = await _scheduler.GetTriggersOfJob(key);
            return triggers;
        }

        public async Task<bool> InterruptAll()
        {
            var jobs = await GetRunningJobsList();
            var interruptTasks = jobs.Select(x => InterruptCommandAsync(x.Key.Key));
            var result = await Task.WhenAll(interruptTasks);
            return result.All(x => x);
        }
    }
}
