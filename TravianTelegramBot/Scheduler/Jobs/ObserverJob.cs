using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TravianTelegramBot.Services;
using TTB.Common.Settings;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models.Results;
using TTB.DAL.Models;
using TravianTelegramBot.ReportBuilders;
using TTB.DAL.Repository;
using TravianTelegramBot.Commands;

namespace TravianTelegramBot.Scheduler.Jobs
{
    public class ObserverJob : AbstractJob
    {
        private readonly IMapper _mapper;
        private readonly IVillageRepository _villageRepository;
        private readonly ICommandFactory _commandFactory;

        public ObserverJob(IServiceProvider service) : base(service)
        {
            _mapper = service.GetService<IMapper>();
            _villageRepository = service.GetService<IVillageRepository>();
            _commandFactory = service.GetService<ICommandFactory>();
        }

        protected override async Task ExecuteJob(JobExecutionData jobExecutionData)
        {
            if (jobExecutionData != null)
            {
                await ExecuteJobScenario(new CancellationToken());
            }
            else
            {
                throw new ArgumentNullException(nameof(jobExecutionData), "The provided execution data is null");
            }
        }

        private async Task ExecuteJobScenario (CancellationToken token)
        {
            _logger.LogDebug($"Starting watch API call for BotUserName:[{_botUser.UserName}], TravianUserName:[{_travianUser.UserName}]");
            var status = _travianUser.PlayerData?.Status ?? PlayerStatus.ALL_QUIET;
            try
            {
                // TODO: create update info command
                // create separate methods for update info in user repo
                //_travianUser.PlayerData.TimeZone = watchResponse.Player.TimeZone;
                //_travianUser.PlayerData.Tribe = (Tribe)((int)watchResponse.Player.Tribe);
                BaseScenarioResult result;
                switch (status)
                {
                    case PlayerStatus.WAS_SCANNED:
                    case PlayerStatus.ALL_QUIET:
                        result = await _gameplayClient.RunWatch(_travianUser);
                        var runScan = await WasQuietScenario(result);
                        if (runScan)
                        {
                            result = await _gameplayClient.RunScan(_travianUser);
                            await WasUnderAttackScenario(result);
                        }
                        break;
                    case PlayerStatus.UNDER_ATTACK:
                        result = await _gameplayClient.RunScan(_travianUser);
                        await WasUnderAttackScenario(result);
                        break;
                    default:
                        throw new ArgumentException(nameof(status), "unrecognized player status.");
                }

                if (result.Cookies != null)
                {
                    _travianUser.Cookies = result.Cookies.Select(x => _mapper.Map<CookieModel>(x));
                    await _travianUserRepository.Update(_travianUser);
                } else if (_travianUser.PlayerData.Status != status)
                {
                    await _travianUserRepository.ReplacePlayerData(_travianUser);
                }

                if (result.Villages != null)
                {
                    _travianUser.PlayerData.VillagesIds = result.Villages.Select(x => $"{x.CoordinateX}&{x.CoordinateY}").ToList();
                    await _travianUserRepository.ReplacePlayerDataVillages(_travianUser);
                }

                _logger.LogDebug($"Watch API call finished for BotUserName:[{_botUser.UserName}], _travianUserName:[{_travianUser.UserName}].");
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.BackgroundJobExecutingException, exc, exc.Message);
            }
        }

        private async Task<bool> WasQuietScenario (BaseScenarioResult result)
        {
            if (result.IsUserUnderAttack)
            {
                try
                {
                    var scans = result.Scans;  

                    if (scans.Any())
                    {
                        foreach (var scan in scans)
                        {
                            var scanReport = ScanReportBuilder
                                .Create(_travianUser.UserName, _travianUser.PlayerData.TimeZone)
                                .WithNotification(scan)
                                .ReportText;

                            await _bot.SendTextMessageAsync(
                                _botUser.ChatId,
                                scanReport);
                        }

                        _travianUser.PlayerData.Status = PlayerStatus.WAS_SCANNED;
                    }
                    else
                    {
                        _travianUser.PlayerData.Status = PlayerStatus.ALL_QUIET;
                    }

                    if (result.IsUserUnderAttack)
                    {
                        var attacks = result.Villages
                            .SelectMany(x => x.Attacks)
                            .OrderBy(x => x.DateTime)
                            .ToList();
                        foreach (var attack in attacks)
                        {
                            var report = IncomingAttackReportBuilder.Create(_travianUser.UserName, _travianUser.PlayerData.TimeZone);
                            report = report.WithNotification(attack);
                            await _bot.SendTextMessageAsync(
                                _botUser.ChatId,
                                report.ReportText);
                        }

                        _travianUser.PlayerData.Status = PlayerStatus.UNDER_ATTACK;
                    }
                }
                catch (Exception exc)
                {
                    await _bot.SendTextMessageAsync(
                            _botUser.ChatId,
                            $"Unable to find any notification for travian user [{_travianUser.UserName}]. Error code: [{12}]");

                    _logger.LogError(12, $"BotUserName:[{_botUser.UserName}], TravianUserName:[{_travianUser.UserName}] " +
                        $"{exc.Message}: \r\n {exc.StackTrace}");
                }
            }

            if (!result.Success)
            {
                await _bot.SendTextMessageAsync(
                    _botUser.ChatId,
                    string.Join(", ", result.Errors));
            }

            return result.IsUserUnderAttack;
        }

        private async Task WasUnderAttackScenario (BaseScenarioResult result)
        {
            if (result.Errors.Any())
            {
                await _bot.SendTextMessageAsync(
                    _botUser.ChatId,
                    string.Join(", ", result.Errors));
                return;
            }

            var currentVillagesData = await _villageRepository.GetVillages(_travianUser.UserName);
            var update = result.Villages.Select(x => _mapper.Map<VillageModel>(x));
            var attacks = result.Villages
                            .Where(x => x.Attacks != null)
                            .SelectMany(x => x.Attacks)
                            .Select(x => _mapper.Map<AttackModel>(x))
                            .OrderBy(x => x.DateTime)
                            .ToList();

            if (attacks != null && attacks.Any())
            {
                var resultVillages = result.Villages.Select(x => _mapper.Map<VillageModel>(x));
                var newAttacks = GetNewAttacks(resultVillages, currentVillagesData).ToList();
                if (newAttacks.Any())
                {
                    await _bot.SendTextMessageAsync(
                        _botUser.ChatId,
                        $"New incoming attacks discovered for player [{_travianUser.UserName}]");

                    try
                    {
                        foreach (var newAttack in newAttacks)
                        {
                            var msg = ArmyEventReportBuilder
                                .ForVillage(newAttack.VillageName, _travianUser.PlayerData.TimeZone)
                                .WithNotification(newAttack)
                                .ReportText;

                            await _bot.SendTextMessageAsync(
                                        _botUser.ChatId,
                                        msg);
                        }

                        await _villageRepository.UpdateWatchInfo(update);
                        foreach (var newAttack in newAttacks)
                        {
                            var cmd = _commandFactory.GetQueueableCommand(nameof(PrepareToAttackCommand), _botUser.ChatId);
                            var attackDt = newAttack.DateTime;
                            var diff = TimeSpan.FromMinutes(5);
                            cmd.Start = attackDt - DateTimeOffset.UtcNow > diff ? attackDt - diff : DateTimeOffset.UtcNow;
                            await cmd.Execute();
                        }
                    }
                    catch (Exception exc)
                    {

                        await _bot.SendTextMessageAsync(
                                _botUser.ChatId,
                                $"Unable to find or update army events for travian user [{_travianUser.UserName}]. Error code: [{LoggingEvents.DbOpertationException}]");

                        _logger.LogError(LoggingEvents.DbOpertationException, exc, $"BotUserName:[{_botUser.UserName}], TravianUserName:[{_travianUser.UserName}], error: {exc.Message}");
                    }
                }
                else
                {
                    await _villageRepository.UpdateWatchInfo(update);
                }
            }
            else
            {
                await _villageRepository.UpdateWatchInfo(update);
                _travianUser.PlayerData.Status = PlayerStatus.ALL_QUIET;
            }

        }

        private IEnumerable<AttackModel> GetNewAttacks(IEnumerable<VillageModel> scanResult, IEnumerable<VillageModel> current)
        {
            var attacksFromScan = scanResult.Where(x => x.Attacks != null).SelectMany(x => x.Attacks).ToList();
            var currentAttacks = current.Where(x => x.Attacks != null).SelectMany(x => x.Attacks).ToList();

            var result = attacksFromScan.Where(x => !currentAttacks.Any(y =>
                y.VillageName == x.VillageName &&
                y.DateTime == x.DateTime &&
                y.IntruderVillageUrl == x.IntruderVillageUrl)).ToList();

            return result;
        }
    }
}
