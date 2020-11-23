using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Quartz;
using TTB.Common.Settings;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class MasterJob : IJob
    {
        private readonly IServiceProvider _service;
        private readonly ILogger<MasterJob> _logger;

        public MasterJob(IServiceProvider service)
        {
            _service = service;
            _logger = service.GetService<ILogger<MasterJob>>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            using (var scope = _service.CreateScope())
            {
                try
                {
                    var jobExecutionData = jobDataMap[AbstractJob.JobExecutionDataKey] as JobExecutionData;
                    var job = Activator.CreateInstance(jobExecutionData.JobType, scope.ServiceProvider) as AbstractJob;
                    await job.Execute(jobExecutionData);
                }
                catch (Exception exc)
                {
                    _logger.LogError(LoggingEvents.BackgroundJobExecutingException, exc, "Unexpected error occurred during the job execution.");
                }
            }
        }
    }
}
