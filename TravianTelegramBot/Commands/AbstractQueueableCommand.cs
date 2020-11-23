
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TravianTelegramBot.Client;
using TravianTelegramBot.Scheduler;
using TravianTelegramBot.Scheduler.Jobs;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Commands
{
    public abstract class AbstractQueueableCommand : AbstractCommand, IQueueableCommand
    {
        protected readonly ITravianUserRepository _travianUserRepository;
        protected readonly ISchedulerService _schedulerService;
        protected readonly IGameplayClient _gameplayClient;

        public AbstractQueueableCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            _travianUserRepository = service.GetService<ITravianUserRepository>();
            _schedulerService = service.GetService<ISchedulerService>();
            _gameplayClient = service.GetService<IGameplayClient>();
        }

        public virtual string Cron => string.Empty;
        public virtual DateTimeOffset Start { get { return DateTimeOffset.UtcNow; } set { } }
        public abstract Type JobType { get; }

        protected sealed override async Task ExecuteCommand(string parameters = "")
        {
            var jobData = new JobExecutionData
            {
                Cron = Cron,
                JobType = JobType,
                Start = Start,
                TravianUser = _travianUser
            };

            var commandKey = SchedulerService.BuildJobKey(jobData);
            await CleanContext(commandKey);
            var isRunning = await _schedulerService.IsJobRunning(commandKey);
            if (_travianUser.ExecutionContext == null)
            {
                _travianUser.ExecutionContext = new ExecutionContextModel
                {
                    Commands = new List<CommandModel>(),
                    IsUserNotified = false
                };
            }else if (_travianUser.ExecutionContext.Commands == null)
            {
                _travianUser.ExecutionContext.Commands = new List<CommandModel>();
            }
            if (_travianUser.PlayerData == null || string.IsNullOrEmpty(_travianUser.PlayerData?.TimeZone) || _travianUser.PlayerData.Tribe == Tribe.NOT_SPECIFIED)
            {
                var updateInfoResult = await _gameplayClient.RunUpdateUserInfo(_travianUser);
                _travianUser.PlayerData = new PlayerDataModel
                {
                    Status = PlayerStatus.ALL_QUIET,
                    Tribe = (Tribe)updateInfoResult.Player.Tribe,
                    TimeZone = updateInfoResult.Player.TimeZone,
                    // TODO: add alliance
                    UserName = _travianUser.UserName                    
                };
            }

            string msg;
            if (parameters.Contains("stop"))
            {
                if (isRunning)
                {
                    var stopResult = await _schedulerService.InterruptCommandAsync(commandKey);
                    if (stopResult)
                    {
                        var i = _travianUser.ExecutionContext.Commands.FindIndex(
                            x => x.Name == Name &&
                            x.KeyGroup == commandKey.Group &&
                            x.KeyName == commandKey.Name);

                        if (i > -1)
                        {
                            _travianUser.ExecutionContext.Commands[i].IsRunning = false;
                            await _travianUserRepository.Update(_travianUser);
                        }
                    }

                    msg = stopResult
                        ? $"The {Name} command for player {_travianUser.UserName} has been stopped."
                        : $"Can not stop the {Name} command for player {_travianUser.UserName}.";
                }
                else
                {
                    msg = $"The {Name} command is not running for player {_travianUser.UserName}.";
                }
            }
            else
            {
                if (isRunning)
                {
                    msg = $"The {Name} command is already being executing for player {_travianUser.UserName}.";
                }
                else
                {
                    msg = $"Starting the {Name} command for player {_travianUser.UserName}.";

                    await _schedulerService.ScheduleCommandAsync(jobData, new CancellationToken());

                    var i = _travianUser.ExecutionContext?.Commands?.FindIndex(
                        x => x.Name == Name &&
                        x.KeyGroup == commandKey.Group &&
                        x.KeyName == commandKey.Name &&
                        x.StartDateTime == Start);
                    if (i.HasValue && i > -1)
                    {
                        _travianUser.ExecutionContext.Commands[i.Value].IsRunning = true;
                    }
                    else
                    {
                        _travianUser.ExecutionContext.Commands.Add(new CommandModel
                        {
                            Name = Name,
                            IsRunning = true,
                            KeyName = commandKey.Name,
                            KeyGroup = commandKey.Group,
                            StartDateTime = Start
                        });
                    }

                    await _travianUserRepository.Update(_travianUser);
                }
            }

            await _bot.SendTextMessageAsync(_chatId, msg);
        }

        private async Task CleanContext(JobKey key)
        {
            var removed = _travianUser.ExecutionContext?.Commands?.RemoveAll(x => x.KeyGroup == key.Group && x.StartDateTime < DateTimeOffset.Now);
            if (removed > 0)
            {
                await _travianUserRepository.Update(_travianUser);
            }
        }
    }
}
