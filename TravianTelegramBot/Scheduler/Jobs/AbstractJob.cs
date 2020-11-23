using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;
using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Services;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public abstract class AbstractJob
    {
        public static readonly string JobExecutionDataKey = "JOB_EXECUTION_DATA";

        protected readonly IBotService _bot;
        protected readonly ITravianUserRepository _travianUserRepository;
        protected readonly IGameplayClient _gameplayClient;

        protected ILogger<AbstractJob> _logger;
        protected BotUser _botUser;
        protected TravianUser _travianUser;

        private readonly IBotUserProvider _botUserProvider;

        public AbstractJob(IServiceProvider service)
        {
            _logger = service.GetService<ILogger<AbstractJob>>();
            _travianUserRepository = service.GetService<ITravianUserRepository>();
            _gameplayClient = service.GetService<IGameplayClient>();
            _botUserProvider = service.GetService<IBotUserProvider>();
            _bot = service.GetService<IBotService>();
        }

        public async Task Execute(JobExecutionData jobExecutionData)
        {
            try
            {
                _botUser = await _botUserProvider.FindByNameAsync(jobExecutionData.TravianUser.BotUserName);
                _travianUser = jobExecutionData.TravianUser;
            }
            catch(Exception exc)
            {
                this._logger.LogError(LoggingEvents.BackgroundJobExecutingException, exc, "Unable to get parameters from JobExecutionContext.");
                return;
            }

            try
            {
                await ExecuteJob(jobExecutionData);
            }
            catch(Exception exc)
            {
                this._logger.LogError(LoggingEvents.BackgroundJobExecutingException, exc, "Unexpected error occurred during the job execution.");
            }
        }

        protected abstract Task ExecuteJob(JobExecutionData jobExecutionData);
    }
}
