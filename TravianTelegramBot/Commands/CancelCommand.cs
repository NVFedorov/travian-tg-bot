using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TravianTelegramBot.Scheduler;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Commands
{
    public class CancelCommand : AbstractCommand
    {
        private readonly ISchedulerService schedulerService;
        protected readonly ITravianUserRepository travianUserRepository;

        public CancelCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            this.schedulerService = service.GetService<ISchedulerService>();
            this.travianUserRepository = service.GetService<ITravianUserRepository>();
        }

        public override string Name => nameof(CancelCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            var param = parameters.Split(' ');
            if (param.Count() == 2)
            {
                param = param.Last().Split('&');
                if (param.Count() == 2)
                {
                    var keyName = param.First();
                    var keyGroup = param.Last();
                    var commandKey = new JobKey(keyName, keyGroup);

                    var stopResult = await this.schedulerService.InterruptCommandAsync(commandKey);
                    if (stopResult)
                    {
                        var i = this._travianUser.ExecutionContext.Commands.FindIndex(
                            x => x.Name == this.Name &&
                            x.KeyGroup == commandKey.Group &&
                            x.KeyName == commandKey.Name);

                        if (i > -1)
                        {
                            this._travianUser.ExecutionContext.Commands[i].IsRunning = false;
                            await this.travianUserRepository.Update(this._travianUser);
                            await this._bot.Client.SendTextMessageAsync(this._chatId, $"The {this.Name} command has been stopped for player {_travianUser.UserName}.");
                        }
                    }
                    else
                    {
                        await this._bot.Client.SendTextMessageAsync(this._chatId, $"Can not stop the {this.Name} command for player {_travianUser.UserName}.");
                    }
                }
                else
                {
                    await this._bot.Client.SendTextMessageAsync(this._chatId, "Unable to parse command parameters");
                }
            }
            else
            {
                await this._bot.Client.SendTextMessageAsync(this._chatId, "Unable to parse command parameters");
            }
        }
    }
}
