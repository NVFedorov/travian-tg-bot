using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Identity;
using TTB.Common.Settings;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Services.Impl
{
    public class QueuedHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public QueuedHostedService(
            IScheduler scheduler,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<QueuedHostedService> logger)
        {
            Scheduler = scheduler;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public IScheduler Scheduler { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting the scheduler.");
            await Scheduler.Start(cancellationToken);

            //_logger.LogInformation("Queued Hosted Service is restoring jobs.");
            //try
            //{
            //    using (var scope = _serviceScopeFactory.CreateScope())
            //    {
            //        var travianUserRepository = scope.ServiceProvider.GetService<ITravianUserRepository>();
            //        var commandFactory = scope.ServiceProvider.GetService<ICommandFactory>();
            //        var userManager = scope.ServiceProvider.GetService<UserManager<BotUser>>();
            //        var users = await Task.Run(() => userManager.Users.ToList());
            //        foreach (var user in users)
            //        {
            //            var player = await travianUserRepository.GetActiveUser(user.UserName);
            //            var commandModels = player?.ExecutionContext?.Commands?.Where(x => x.IsRunning);
            //            if (commandModels != null)
            //            {
            //                var commands = new List<IQueueableCommand>();
            //                commandModels.Where(x => !x.Name.ToLower().Contains("watch") && x.IsRunning).ToList().ForEach(x =>
            //                {
            //                    var cmd = commandFactory.GetQueueableCommand(x.Name, user.ChatId);
            //                    cmd.Start = x.StartDateTime;
            //                    commands.Add(cmd);
            //                });

            //                await Task.WhenAll(commands.Select(x => x.Execute()));
            //                _logger.LogInformation("Queued Hosted Service completed restoring jobs successfuly.");
            //            }
            //        }
            //    }
            //}
            //catch (Exception exc)
            //{
            //    _logger.LogError(LoggingEvents.BackgroundJobCreationException, exc, $"Queued Hosted Service is unable to restore jobs: {exc.Message}");
            //}

            _logger.LogInformation("Queued Hosted Service has started the scheduler.");
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is shutting down the scheduler.");
            await Scheduler?.Shutdown(cancellationToken);
            _logger.LogInformation("Queued Hosted Service has shut the scheduler down.");
        }
    }
}
