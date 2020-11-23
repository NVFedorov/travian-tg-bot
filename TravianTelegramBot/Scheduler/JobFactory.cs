using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using TravianTelegramBot.Scheduler.Jobs;

namespace TravianTelegramBot.Scheduler
{
    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _service;
        private readonly ILogger<JobFactory> _logger;

        public JobFactory(IServiceProvider service)
        {
            _service = service;
            _logger = service.GetService<ILogger<JobFactory>>();
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobType = bundle.JobDetail.JobType;
            return Activator.CreateInstance(jobType, _service) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            var disposableJob = job as IDisposable;
            if (disposableJob != null)
            {
                _logger.LogDebug("Disposing the job");
                disposableJob.Dispose();
            }
            else
            {
                _logger.LogDebug("The job is not dispsable");
            }

        }
    }
}
