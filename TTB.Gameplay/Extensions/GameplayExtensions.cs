using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TTB.Gameplay.API;
using TTB.Gameplay.Services;
using TTB.Gameplay.Services.Impl;
using TTB.Gameplay.Settings;

namespace TTB.Gameplay.Extensions
{
    public static class GameplayExtensions
    {
        public static IServiceCollection AddGameplay(this IServiceCollection services, IConfiguration config)
        {            
            services.Configure<WebDriverSettings>(config);
            services.AddSingleton<IWebDriverProvider, WebDriverProvider>();
            services.AddSingleton<IScenarioExecutor, ScenarioExecutor>();
            services.AddSingleton<IScenarioFactory, ScenarioFactory>();
            services.AddSingleton<IGameplayApi, GameplayApi>();

            return services;
        }
    }
}
