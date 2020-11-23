using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDbGenericRepository;
using Newtonsoft.Json;
using TravianTelegramBot.Identity;
using TravianTelegramBot.Serializers;
using TravianTelegramBot.Settings;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;

namespace TravianTelegramBot.Extensions
{
    public static class MongoExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection services, IHostingEnvironment env, IConfiguration config)
        {
            var dbConfig = config.GetSection(nameof(DatabaseSettings));
            var connectionString = dbConfig.GetValue<string>("ConnectionString");
            var dbName = dbConfig.GetValue<string>("DatabaseName");

            var context = new MongoDbContext(connectionString, dbName);

            BsonSerializer.RegisterSerializer(typeof(DateTime), new MongoDbDateTimeSerializer());
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), new DateTimeOffsetSerializer(BsonType.Document));
            services.AddScoped(cfg => context.Database);
            services.AddIdentity<BotUser, BotUserRole>()
                    .AddMongoDbStores<BotUser, BotUserRole, string>(context)
                    .AddDefaultTokenProviders();
            var packEnum = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            };
            ConventionRegistry.Register("EnumStringConvention", packEnum, t => true);

            var packCamelCase = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("camel case",
                                        packCamelCase,
                                        t => t.FullName.StartsWith("TTB.DAL.Models"));

            EnsureCollectionExists<UnitModel>(context.Database, env, "units").Wait();
            EnsureCollectionExists<BuildingModel>(context.Database, env, "buildings").Wait();
            EnsureCollectionExists<BuildingPlanModel>(context.Database, env, "buildingPlans").Wait();

            return services;
        }

        public static async Task<bool> CollectionExistsAsync(this IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            //filter by collection name
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            //check for existence
            return await collections.AnyAsync();
        }

        public static async Task<bool> CreateCollectionFromFile<T>(this IMongoDatabase database, string collectionName, string filePath)
        {
            var reader = new StreamReader(filePath);
            var json = reader.ReadToEnd();
            var units = JsonConvert.DeserializeObject<List<T>>(json);
            
            await database.CreateCollectionAsync(collectionName);

            var collection = database.GetCollection<T>(collectionName);
            await collection.InsertManyAsync(units);

            return true;
        }

        private static async Task EnsureCollectionExists<T>(IMongoDatabase database, IHostingEnvironment env, string collectionName)
        { 
            if (!await database.CollectionExistsAsync(collectionName))
            {
                var filepath = Path.Combine(env.WebRootPath, "data", "json", $"{collectionName}.json");
                await database.CreateCollectionFromFile<T>(collectionName, filepath);
            }
        }
    }
}
