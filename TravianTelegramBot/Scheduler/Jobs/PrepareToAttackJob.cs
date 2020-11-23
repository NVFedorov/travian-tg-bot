using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TravianTelegramBot.Providers;
using TTB.Common.Settings;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Common.Extensions;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class PrepareToAttackJob : AbstractJob
    {
        private readonly IActionProvider _actionProvider; 

        public PrepareToAttackJob(IServiceProvider service) : base(service)
        {
            _actionProvider = service.GetService<IActionProvider>();
        }

        protected override async Task ExecuteJob(JobExecutionData jobExecutionData)
        {
            await CleanContext(jobExecutionData);
            try
            {
                var actions = await this._actionProvider.GetActionsForPlayer(_travianUser);
                if (actions != null && actions.Any())
                {
                    _logger.LogDebug($"Starting Prepare to attack command for BotUserName:[{_botUser.UserName}], TravianUserName:[{_travianUser.UserName}]");
                    var response = await _gameplayClient.ExecuteActions(_travianUser, actions);

                    if (response.Errors != null && response.Errors.Any(x => x != null))
                    {
                        var villagesNames = string.Join(", ", actions.Select(x => x.Village.Name));
                        await _bot.SendTextMessageAsync(_botUser.ChatId, $"The prepare command completed with errors");
                        _logger.LogError(LoggingEvents.BackgroundJobCreationException, string.Join(", ", response.Errors));
                        var notFound = response.Errors.Where(x => x.ErrorType == "NotFound");
                        if (notFound.Any())
                        {
                            foreach (var error in notFound)
                            {
                                await _bot.SendTextMessageAsync(_botUser.ChatId, error.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        foreach (var action in actions)
                        {
                            var msg = $"The village {action.Village.Name} was prepared to attack with following action: [{action.Action.GetEnumDisplayName()}]";
                            if (action is SendResourcesAction)
                            {
                                var a = action as SendResourcesAction;
                                msg += $" to {a.To.Name}";
                            }

                            msg += ".";
                            await _bot.SendTextMessageAsync(_botUser.ChatId, msg);
                        }
                    }

                    _logger.LogDebug($"Prepare to attack command completed for player :[{_travianUser.UserName}]");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.BackgroundJobCreationException, exc, exc.Message);
            }
        }

        private async Task CleanContext(JobExecutionData jobExecutionData)
        {
            var key = SchedulerService.BuildJobKey(jobExecutionData);
            var removed = _travianUser.ExecutionContext?.Commands?.RemoveAll(x => x.KeyGroup == key.Group && x.StartDateTime < DateTimeOffset.Now);
            if (removed > 0)
            {
                await _travianUserRepository.Update(_travianUser);
            }
        }
    }
}
