using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TTB.Common.Settings;
using TTB.DAL.Models;
using TTB.DAL.Models.ScenarioModels;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Scenarios;
using TTB.Gameplay.Scenarios.Impl;
using TTB.Gameplay.Services;
using TTB.Gameplay.Services.Impl;

namespace TTB.Gameplay.API
{
    public class GameplayApi : IGameplayApi
    {
        private readonly IScenarioExecutor _scenarioExecutor;
        private readonly IScenarioFactory _scenarioFactory;
        private readonly ILogger<GameplayApi> _logger;
        private readonly ScenarioDecoratorBuilder _scenarioBuilder;

        public GameplayApi(IScenarioExecutor scenarioExecutor, IScenarioFactory scenarioFactory, ILogger<GameplayApi> logger)
        {
            this._scenarioExecutor = scenarioExecutor;
            this._scenarioFactory = scenarioFactory;
            this._logger = logger;
            this._scenarioBuilder = new ScenarioDecoratorBuilder(this._scenarioFactory);
        }

        public async Task<BaseScenarioResult> Scan(Player player, bool details)
        {
            var scenario = this._scenarioBuilder
                   .WithScenario<IncomingAttacksScenario>()
                   .Build();

            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }

        public async Task<BaseScenarioResult> Watch(Player player)
        {
            var scenario = this._scenarioBuilder
                .WithScenario<WatchScenario>()
                .WithScenario<CheckReportsScenario>()
                .Build();

            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }
        public async Task<BaseScenarioResult> UpdateUserInfo(Player player)
        {
            var scenario = this._scenarioBuilder
                .WithScenario<UpdateUserInfoScenario>()
                .Build();

            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }

        public async Task<BaseScenarioResult> Logout(Player player)
        {
            var scenario = this._scenarioBuilder
                   .WithScenario<LogoutScenario>()
                   .Build();

            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }

        public async Task<BaseScenarioResult> GetTargetsFromMessage(Player player, string messageUrl)
        {
            var scenario = this._scenarioBuilder
                   .WithScenario<ParseSpamTargetsScenario>()
                   .Build();

            var context = new ScenarioContext { Player = player, StartUrl = messageUrl };
            return await this.RunScenario(scenario, context);
        }

        private async Task<BaseScenarioResult> RunScenario(IScenario scenario, ScenarioContext context)
        {
            BaseScenarioResult result;
            try
            {
                result = await Task.Run(() => this._scenarioExecutor.ExecuteScenario(scenario, context));
                result.Success = !result.Errors.Any();
            }
            catch (Exception exc)
            {
                result = new BaseScenarioResult();
                result.Errors.Add(new ScenarioError
                {
                    ErrorMessage = $"Unable to execute scenario. Error code: {LoggingEvents.ScenarioException}",
                    ErrorSource = $"{this.GetType().Name}::{MethodBase.GetCurrentMethod().Name}",
                    ErrorType = "Critical"
                });

                this._logger.LogError(LoggingEvents.ScenarioException, exc, exc.Message);
            }

            return result;
        }

        public async Task<BaseScenarioResult> RunScenarioWithActions<T>(Player player, IEnumerable<T> actions) where T: GameAction
        {
            var scenarioBuilder = this._scenarioBuilder;
            foreach (var action in actions)
            {
                scenarioBuilder = scenarioBuilder.WithScenario(action);
            }

            var scenario = scenarioBuilder.Build();
            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }

        public async Task<BaseScenarioResult> GetVillagesInfo(Player player)
        {
            var scenario = this._scenarioBuilder
                      .WithScenario<GetVillageInfoScenario>()
                      .Build();

            var context = new ScenarioContext { Player = player };
            return await this.RunScenario(scenario, context);
        }
    }
}
