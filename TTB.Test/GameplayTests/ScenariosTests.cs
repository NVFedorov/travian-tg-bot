using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using TTB.Gameplay.API;
using TTB.Gameplay.Models;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.ContextModels.Actions.Enums;
using TTB.Gameplay.Models.Results;
using TTB.Gameplay.Scenarios.Impl;
using TTB.Gameplay.Services;
using TTB.Gameplay.Services.Impl;
using TTB.Gameplay.Settings;
using TTB.Test.Fake;

namespace TTB.Test.GameplayTests
{
    //[Ignore("Selenium test")]
    [TestFixture]
    public class ScenariosTests
    {
        private IWebDriverProvider provider;
        private Player player;
        private Mock<ILogger<GameplayApi>> logger;

        [OneTimeSetUp]
        public void SetUp()
        {
            var absolutePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\tools\webdriver"));
            var options = new Mock<IOptionsMonitor<WebDriverSettings>>();
            options.Setup(x => x.CurrentValue).Returns(new WebDriverSettings { LocalDriverPath = absolutePath });
            provider = new WebDriverProvider(options.Object);
            logger = new Mock<ILogger<GameplayApi>>();
            
            // test data should be loaded from a local file
            player = new Player
            {
                Uri = new Uri(""),
                TimeZone = "UTC+2"
            };
        }

        [Test]
        public void TestBaseScenario()
        {
            var watch = new WatchScenario(provider);
            var check = new CheckReportsScenario(provider);
            watch.Decorate(check);

            Assert.DoesNotThrow(() => watch.Execute(new ScenarioContext { Player = player }));
        }

        [Test]
        public async Task TestWatchScenario()
        {
            var factory = new ScenarioFactory(provider);
            var executor = new ScenarioExecutor();
            var api = new GameplayApi(executor, factory, logger.Object);

            BaseScenarioResult result = await api.Watch(player);
            Assert.IsTrue(result.IsUserUnderAttack);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task TestCheckReportsScenario()
        {
            var factory = new ScenarioFactory(provider);
            var executor = new ScenarioExecutor();
            var api = new GameplayApi(executor, factory, logger.Object);

            BaseScenarioResult result = await api.Watch(player);
            Assert.IsTrue(result.IsUserUnderAttack);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("No scan events found", result.Messages.FirstOrDefault());
        }

        [Test]
        public async Task TestIncomingAttackScenario()
        {
            var factory = new ScenarioFactory(provider);
            var executor = new ScenarioExecutor();
            var api = new GameplayApi(executor, factory, logger.Object);

            BaseScenarioResult result = await api.Scan(player, false);

            Assert.IsTrue(result.IsUserUnderAttack);
            Assert.IsTrue(result.Villages.Count > 0);
            Assert.IsNotNull(result.Villages.FirstOrDefault());
            Assert.IsTrue(result.Villages.FirstOrDefault().IsUnderAttack);
            Assert.IsTrue(result.Villages.FirstOrDefault().Attacks.Any());
        }

        [Test]
        [TestCase(false, 2, 2, "send_to_any_natar")]
        [TestCase(false, 2, 2, "whatever")]
        //[TestCase(true, -1, -1)]
        public void TestSendArmyScenario(bool sendAll, int existing, int existing2, string villageName)
        {
            var action = new SendArmyAction
            {
                Action = GameActionType.SEND_ARMY,
                Village = new Village
                {
                    Name = "Unex_Test_Village"
                },
                SendAll = sendAll,
                To = new Village
                {
                    Name = villageName,
                    CoordinateX = -46,
                    CoordinateY = 33
                },
                Type = SendArmyType.RAID,
                UnitsToSend = new Dictionary<string, int>
                {
                    { "Фаланга", existing },
                    { "Мечник", existing2 },
                    { "any", -1 },
                }
            };

            var send = new SendArmyScenario(provider, action);

            Assert.DoesNotThrow(() => send.Execute(new ScenarioContext { Player = player }));
        }

        [Test]
        public void TestPrepareToAttackScenarios()
        {
            var factory = new ScenarioFactory(provider);
            var executor = new ScenarioExecutor();
            var api = new GameplayApi(executor, factory, logger.Object);
            var village = new Village
            {
                Name = "Unex_Test_Village"
            };
            var to = new Village
            {
                CoordinateX = -46,
                CoordinateY = 33
            };

            var actions = new List<GameAction>
            {
                new SendResourcesAction
                {
                    Village = village,
                    To = to,
                    Action = GameActionType.SEND_RESOURCES
                },
                new TrainArmyAction()
                {
                    Village = village,
                    Action = GameActionType.TRAIN_ARMY,
                    UnitsToTrain = new Dictionary<string, int>{{ "Фаланга", 1}}
                },
                new SendArmyAction()
                {
                    Village = village,
                    To = to,
                    Action = GameActionType.SEND_ARMY,
                    UnitsToSend = new Dictionary<string, int>{{ "Фаланга", 1}},
                    Type = SendArmyType.RAID
                }
            };

            Assert.DoesNotThrow(() => api.RunScenarioWithActions(player, actions).Wait());
        }

        [Test]
        public void TestFarmList()
        {
            var farm = new FindFarmTargetsScenario(provider);
            BaseScenarioResult result = new BaseScenarioResult();
            Assert.DoesNotThrow(() =>
            {
                result = farm.Execute(new ScenarioContext { Player = player });
            });


            var json = JsonConvert.SerializeObject(new
            {
                server = player.Uri.AbsolutePath,
                farmList = result.Villages
            }, 
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\farm.json");
            using (var writer = File.CreateText(path))
            {
                writer.Write(json);
            }
        }

        [Test]
        public void TestParseMessageScenario()
        {
            const string messageUrl = "https://ts2.travian.ru/messages.php?t=0&order=DESC&page=1&id=303073";
            var parse = new ParseSpamTargetsScenario(provider);
            BaseScenarioResult result = new BaseScenarioResult();
            Assert.DoesNotThrow(() =>
            {
                result = parse.Execute(new ScenarioContext { Player = player, StartUrl = messageUrl });
            });

            Assert.NotNull(result.Villages);
            Assert.AreEqual(3, result.Villages.Count);
            Assert.AreEqual(-46, result.Villages.First().CoordinateX);
            Assert.AreEqual(33, result.Villages.First().CoordinateY);
        }

        [Test]
        public void UpdateVillageDataTest()
        {
            var village = new Village
            {
                Name = "Unexpected_Town_15"
            };

            var scenario = new GetVillageInfoScenario(provider);
            BaseScenarioResult result = new BaseScenarioResult();
            Assert.DoesNotThrow(() =>
            {
                result = scenario.Execute(new ScenarioContext { Player = player, Actions = new List<BuildAction>() });
            });

            Assert.NotNull(result.Villages);
        }

        [Test]
        [TestCase(false, 10, 2, "whatever")]
        //[TestCase(true, -1, -1)]
        public void TestSendWavesScenario(bool sendAll, int existing, int existing2, string villageName)
        {
            var action = new SendWavesAction
            {
                Action = GameActionType.SEND_ARMY,
                Village = new Village
                {
                    Name = "Unexpected_Town_02"
                },
                SendAll = sendAll,
                To = new Village
                {
                    Name = villageName,
                    CoordinateX = -151,
                    CoordinateY = 118
                },
                Type = SendArmyType.ATTACK,
                UnitsToSend = new Dictionary<string, int>
                {
                    { "Легионер", existing }
                },
                Waves = 4
            };

            var send = new SendWavesScenario(provider, action);
            BaseScenarioResult result = new BaseScenarioResult();

            Assert.DoesNotThrow(() => result = send.Execute(new ScenarioContext { Player = player }));

            Assert.AreEqual(4, result.Villages.Count);
        }

        [Test]
        public async Task TestBuildScenario()
        {
            var village = new Village
            {
                Name = "Unexpected_Town_11"
            };

            var buildingRepo = BuildingsProviderFake.GetBuildingRepository();
            var buildings = (await buildingRepo.GetAllBuildings()).ToList();
            var woodcutter = buildings.First(x => x.Name == "woodcutter");
            var action = new BuildAction
            {
                Action = GameActionType.BUILD,
                BuildingId = woodcutter.BuildingId,
                BuildingSlot = "buildingSlot14",
                Village = village
            };

            var scenario = new BuildScenario(provider, action);
            BaseScenarioResult result = new BaseScenarioResult();
            //Assert.DoesNotThrow(() =>
            //{
            //    result = scenario.Execute(new ScenarioContext { Player = player, Actions = new List<BuildAction>() });
            //});

            //Assert.AreEqual(0, result.Errors.Count);

            action = new BuildAction
            {
                Action = GameActionType.BUILD,
                BuildingId = buildings.First(x => x.Name == "warehouse").BuildingId,
                BuildingSlot = "a30",
                Village = village
            };

            scenario = new BuildScenario(provider, action);
            result = new BaseScenarioResult();
            //Assert.DoesNotThrow(() =>
            //{
            //    result = scenario.Execute(new ScenarioContext { Player = player, Actions = new List<BuildAction>() });
            //});

            //Assert.AreEqual(0, result.Errors.Count);

            var sawmill = buildings.First(x => x.Name == "granary");
            action = new BuildAction
            {
                Action = GameActionType.BUILD,
                BuildingId = sawmill.BuildingId,
                BuildingSlot = "aid21",//sawmill.PrefferedBuildingSlot,
                Village = village
            };

            scenario = new BuildScenario(provider, action);
            result = new BaseScenarioResult();
            Assert.DoesNotThrow(() =>
            {
                result = scenario.Execute(new ScenarioContext { Player = player, Actions = new List<BuildAction>() });
            });

            Assert.AreEqual(0, result.Errors.Count);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            var driver = provider.GetWebDriver();
            driver.Quit();
            driver.Dispose();
        }
    }
}
