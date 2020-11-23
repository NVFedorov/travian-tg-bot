using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TravianTelegramBot.Client;
using TravianTelegramBot.Services;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Commands
{
    public class UpdateUserInfoCommand : AbstractCommand
    {
        private readonly IGameplayClient _client;
        private readonly IMapper _mapper;
        private readonly IVillageRepository _villageRepository;
        private readonly ITravianUserRepository _travianUserRepository;
        private readonly IBotService _botService;

        public UpdateUserInfoCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            _client = service.GetService<IGameplayClient>();
            _mapper = service.GetService<IMapper>();
            _villageRepository = service.GetService<IVillageRepository>();
            _travianUserRepository = service.GetService<ITravianUserRepository>();
            _botService = service.GetService<IBotService>();
        }

        public override string Name => nameof(UpdateUserInfoCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            var updateResult = await _client.RunUpdateUserInfo(_travianUser);
            if (updateResult.Errors == null || !updateResult.Errors.Any())
            {
                var update = updateResult.Villages.Select(x => _mapper.Map<VillageModel>(x));
                await _villageRepository.UpdateBaseInfos(update);

                if (_travianUser.PlayerData == null)
                {
                    _travianUser.PlayerData = new PlayerDataModel
                    {
                        Alliance = updateResult.Player.Alliance,
                        TimeZone = updateResult.Player.TimeZone,
                        Tribe = (Tribe)updateResult.Player.Tribe,
                        UserName = _travianUser.UserName,
                        VillagesIds = update.Select(x => x.VillageId).ToList(),
                        Status = PlayerStatus.ALL_QUIET
                    };
                }
                else
                {
                    _travianUser.PlayerData.Alliance = updateResult.Player.Alliance;
                    _travianUser.PlayerData.TimeZone = updateResult.Player.TimeZone;
                    _travianUser.PlayerData.Tribe = (Tribe)updateResult.Player.Tribe;
                }

                await _travianUserRepository.ReplacePlayerData(_travianUser);
                await _botService.SendTextMessageAsync(_chatId, $"User info updated");
            }
            else
            {
                await _botService.SendTextMessageAsync(_chatId, $"Unable to update user info: [{string.Join("], [", updateResult.Errors)}]");
            }            
        }
    }
}
