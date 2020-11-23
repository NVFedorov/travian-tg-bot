using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.ViewModels;
using TTB.DAL.Models;

namespace TravianTelegramBot.Scheduler
{
    public interface ISchedulerService
    {
        Task<JobKey> ScheduleCommandAsync(JobExecutionData data, CancellationToken cancellationToken);
        Task<bool> IsJobRunning(JobKey key);
        Task<bool> InterruptCommandAsync(JobKey key);
        Task<Dictionary<IJobDetail, DateTimeOffset>> GetRunningJobsList();
        Task<IEnumerable<ITrigger>> GetJobTrigger(JobKey key);
        Task PauseAll();
        Task<bool> InterruptAll();
    }
}
