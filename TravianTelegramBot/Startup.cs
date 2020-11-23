using System;

using AutoMapper;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

using MongoDbGenericRepository;

using TravianTelegramBot.Client;
using TravianTelegramBot.Extensions;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Providers.Impl;
using TravianTelegramBot.Scheduler;
using TravianTelegramBot.Serializers;
using TravianTelegramBot.Services;
using TravianTelegramBot.Services.Impl;
using TravianTelegramBot.Settings;
using TravianTelegramBot.Settings.CommandSettings;
using TTB.Common.Extensions;
using TTB.DAL.Models;
using TTB.DAL.Repository;
using TTB.DAL.Repository.Impl;
using TTB.Gameplay.Extensions;
using TTB.Gameplay.Settings;

namespace TravianTelegramBot
{
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            _environment = environment;
            _logger = logFactory.CreateLogger<Startup>();
            _logger.LogInformation($"Application Started at {DateTimeOffset.Now.ToFormattedString()}");
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configs
            services.Configure<BotConfiguration>(Configuration.GetSection(nameof(BotConfiguration)));
            services.Configure<SecretSettings>(Configuration.GetSection(nameof(SecretSettings)));
            services.Configure<WatchCommandConfiguration>(Configuration.GetSection(nameof(WatchCommandConfiguration)));

            // Database
            services.AddMongoDB(_environment, Configuration);

            // Repositories
            services.AddScoped<ITravianUserRepository, TravianUserRepository>();
            services.AddScoped<ILogRepository<ManagerLogEntryModel>, LogRepository<ManagerLogEntryModel>>();
            services.AddScoped<ILogRepository<LogEntryModel>, LogRepository<LogEntryModel>>();
            services.AddScoped<IManagerLogRepository, ManagerLogRepository>();
            services.AddScoped<ISecretRepository, SecretReporitory>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IScanRepository, ScanRepository>();
            services.AddScoped<IArmyEventRepository, ArmyEventRepository>();
            services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
            services.AddScoped<IPlayerDataRepository, PlayerDataRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<IVillageRepository, VillageRepository>();
            services.AddScoped<IBuildingPlanRepository, BuildingPlanRepository>();
            services.AddScoped<IBuildingRepository, BuildingRepository>();

            // Providers
            services.AddScoped<ISecretProvider, SecretProvider>();
            services.AddScoped<IActionProvider, ActionProvider>();
            services.AddScoped<IBotUserProvider, BotUserProvider>();
            services.AddScoped<IJobInfoProvider, JobInfoProvider>();

            // Services
            services.AddSingleton<ISchedulerService, SchedulerService>();
            services.AddScoped<IBotService, BotService>();
            services.AddScoped<ICommandFactory, CommandFactory>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddHostedService<QueuedHostedService>();

            // Gameplay
            services.AddSingleton<IGameplayClient, GameplayClient>();
            services.AddGameplay(Configuration.GetSection(nameof(WebDriverSettings)));

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddQuartz();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _logger.LogInformation($"Application running in HostingEnvironment: [{env.EnvironmentName}]");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Details");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Register}/{id?}");
            });
        }
    }
}
