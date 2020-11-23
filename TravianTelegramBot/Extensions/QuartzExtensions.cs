using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TravianTelegramBot.Scheduler;

namespace TravianTelegramBot.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollection AddQuartz(this IServiceCollection services)
        {
            services.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
                scheduler.JobFactory = new JobFactory(provider.GetService<IServiceProvider>());
                return scheduler;
            });

            return services;
        }
    }
}
