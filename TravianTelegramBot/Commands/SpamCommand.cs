using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravianTelegramBot.Client;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;

namespace TravianTelegramBot.Commands
{
    public class SpamCommand : AbstractCommand
    {
        private readonly IGameplayClient _client;
        private readonly IVillageRepository _villageRepository;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;

        public SpamCommand(IServiceProvider service, long chatId) : base(service, chatId)
        {
            _client = service.GetService<IGameplayClient>();
            _villageRepository = service.GetService<IVillageRepository>();
            _mapper = service.GetService<IMapper>();
            _unitRepository = service.GetService<IUnitRepository>();
        }

        public override string Name => nameof(SpamCommand);

        protected override async Task ExecuteCommand(string parameters = "")
        {
            var param = parameters.Split(' ').ToList();
            if (param.Count == 2 || param.Count == 3)
            {
                await _bot.Client.SendTextMessageAsync(_chatId, $"Starting the {Name}");
                try
                {
                    var messageUrl = param[1];
                    var spamVillages = (await _villageRepository.GetVillages(_travianUser.UserName)).Where(x => x.IsSpamFeatureOn);
                    if (spamVillages != null && spamVillages.Any())
                    {
                        var result = await _client.GetTargetsFromMessage(_travianUser, messageUrl);
                        if (result?.Villages != null && result.Villages.Any())
                        {
                            var max = param.Count == 3 ? int.Parse(param[2]) : result.Villages.Count;
                            if (max > result.Villages.Count)
                                max = result.Villages.Count;

                            var actions = new List<SendArmyAction>();
                            var units = await _unitRepository.GetUnitsByTribe(_travianUser.PlayerData.Tribe);
                            foreach (var village in spamVillages)
                            {
                                var to = new List<SendArmyAction>();
                                for(var i = 0; i < max; i++)
                                {
                                    to.Add(new SendArmyAction
                                    {
                                        Action = GameActionType.SEND_ARMY,
                                        Type = SendArmyType.ATTACK,
                                        Village = _mapper.Map<Village>(village),
                                        To = result.Villages[i],
                                        UnitsToSend = village.SpamUnits.ToDictionary(y => units.FirstOrDefault(z => z.Name == y.Key).LocalizedNameRu, y => y.Value)
                                    });
                                }

                                actions.AddRange(to);
                            }

                            var sendResult = await _client.ExecuteActions(_travianUser, actions);
                            if (sendResult.Success)
                            {
                                await _bot.SendTextMessageAsync(_chatId, $"Sent spam to {string.Join(", ", result.Villages.Select(x => GetVillageDisplayName(x)))}");
                            }
                            else
                            {
                                await _bot.Client.SendTextMessageAsync(_chatId, $"Unable to complete {Name}: {string.Join(", ", sendResult.Errors) }");
                            }
                        }
                        else
                        {
                            await _bot.Client.SendTextMessageAsync(_chatId, $"Unable to parse target villages.");
                        }
                    }
                    else
                    {
                        await _bot.Client.SendTextMessageAsync(_chatId, $"No villages with active spam feature found.");
                    }
                }
                catch (Exception exc)
                {
                    var msg = "Error during spam command execution";
                    _logger.LogError(exc, msg);
                    await _bot.Client.SendTextMessageAsync(_chatId, msg);
                }
            }
        }

        private static string GetVillageDisplayName(Village village)
        {
            return $"{village.Name} ({village.CoordinateX}|{village.CoordinateY})";
        }
    }
}
