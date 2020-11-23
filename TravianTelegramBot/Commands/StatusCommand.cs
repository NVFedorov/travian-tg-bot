using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TravianTelegramBot.Scheduler;
using System.Text;
using TravianTelegramBot.Scheduler.Jobs;
using Microsoft.Extensions.Logging;
using TTB.Common.Extensions;

namespace TravianTelegramBot.Commands
{
    public class StatusCommand : AbstractCommand
    {
        private readonly ISchedulerService _schedulerService;

        public StatusCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            this._schedulerService = service.GetService<ISchedulerService>();
        }

        public override string Name => nameof(StatusCommand);

        protected async override Task ExecuteCommand(string parameters = "")
        {
            this._logger.LogDebug($"The commands status was requested by user [{this._botUser.UserName}] for player [{this._travianUser.UserName}]");
            var runningJobs = await this._schedulerService.GetRunningJobsList();
            var msg = string.Empty;
            if (!runningJobs.Any())
            {
                msg = "No running commands found.";
            }
            else
            {
                await this._bot.Client.SendTextMessageAsync(this._chatId, $"The following commands are running for player [{this._travianUser.UserName}]:");
                foreach (var pair in runningJobs.OrderBy(x => x.Value))
                {
                    var job = pair.Key;
                    var time = pair.Value;
                    if (job.JobDataMap[AbstractJob.JobExecutionDataKey] is JobExecutionData data)
                    {
                        var result = new StringBuilder();
                        result.Append($"{data.JobType.Name}: ");
                        var nextInfo = string.IsNullOrEmpty(data.Cron)
                            ? string.Empty
                            : $"with CRON expression [{data.Cron}]";

                        result.AppendLine(nextInfo);
                        if (time >= DateTimeOffset.Now)
                            result.AppendLine($"Next running time: {time.ToDisplayStringApplyTimeZone(this._travianUser.PlayerData.TimeZone)}");
                        else
                            result.AppendLine($"Next running time: executes now.");
                        await this._bot.Client.SendTextMessageAsync(this._chatId, result.ToString());
                        await this._bot.Client.SendTextMessageAsync(this._chatId, "To cancel the job, send:");
                        await this._bot.Client.SendTextMessageAsync(this._chatId, $"/cancel {job.Key.Name}&{job.Key.Group}");
                    }
                }
            }

            this._logger.LogDebug($"The status command completed for [{this._botUser.UserName}], player [{this._travianUser.UserName}]. " +
                $"The number of running jobs: [{runningJobs.Count()}]");
            if (!string.IsNullOrEmpty(msg))
            {
                await this._bot.Client.SendTextMessageAsync(this._chatId, msg);
            }
        }
    }
}
