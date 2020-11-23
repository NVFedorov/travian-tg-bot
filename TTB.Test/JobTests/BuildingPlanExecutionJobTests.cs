using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TravianTelegramBot.Calculator;
using TravianTelegramBot.Client;
using TravianTelegramBot.Commands;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Scheduler.Jobs;
using TravianTelegramBot.Services;
using TTB.DAL.Models;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Gameplay.Models.Results;
using TTB.Test.Fake;

namespace TTB.Test.JobTests
{
    [TestFixture]
    public class BuildingPlanExecutionJobTests
    {
        private FakeServiceProviderBuilderForJob _serviceProviderBuilder;

        [SetUp]
        public void Setup()
        {
            _serviceProviderBuilder = FakeServiceProviderBuilderForJob.DefaultBuilder()
                .AddBuildingRepository()
                .AddUnitRepository();

            _serviceProviderBuilder.WithService(BuildingPlanProviderFake.GetBuildingRepository());
        }

        // test one village not enough resources -> schdules the job to time when there are enough resources (NOT IMPLEMENTED) 
        // test one village with a building being built -> schedules the job to be executed after the queue will be free
        // test one village with build action returns no erros -> a building is being built, job scheduled to the time when the build is completed
        // test one village with build action returns no space in queue error -> schedules the job to be executed after the queue will be free 
        // test one village with build action returns not enough resources error -> schdules the job to time when there are enough resources (NOT IMPLEMENTED)
        // test one village waiting for resources -> reschedule the job to the next execution time
        // test two villages with build action returns not enough resources -> sends resources and schedules the job execution time to the time when resources are delivered
        // test three villages (2 build feature on, 1 build feature off):
        //      build action returns no space in queue error -> reschedule to the nearest time when there is space in a queue
        //      build action returns not enough resources for one of the villages -> send resources and reschedule to the nearest time 
        //      one village is waiting for resources, one is free, -> do not update info for waiting village, build only in the second
        //      all the villages are waiting for resources -> schedule job to the nearest next execution time

        [Test]
        public void Test_One_Village_With_Building_Being_Built()
        {
            var testVillage = PrepareTestOneVillage();
            var resultVillage = _serviceProviderBuilder.Mapper.Map<Village>(testVillage);
            var finishTime = TimeSpan.FromMinutes(30);
            resultVillage.Dorf1BuildTimeLeft = finishTime;
            resultVillage.CanBuild = false;
            resultVillage.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage.Warehourse = 800;
            resultVillage.Granary = 800;

            var infoResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village> { resultVillage })
                .Build();

            var gamePlayMock = new Mock<IGameplayClient>();
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()))
                .Returns(Task.FromResult(infoResult));
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()))
                .Returns(Task.FromResult(new BaseScenarioResult()));
            _serviceProviderBuilder.WithService(gamePlayMock.Object);

            var cmd = new FakeCommand();
            var cmdMock = new Mock<ICommandFactory>();
            cmdMock.Setup(x => x.GetQueueableCommand(nameof(BuildCommand), It.IsAny<long>()))
                .Returns(cmd);

            _serviceProviderBuilder.WithService(cmdMock.Object);

            var job = new BuildingPlanExecutionJob(_serviceProviderBuilder.Build());
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET, true),
                JobType = typeof(BuildingPlanExecutionJob)
            };

            var now = DateTimeOffset.Now;
            Assert.DoesNotThrowAsync(async () => await job.Execute(data));

            _serviceProviderBuilder.LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);
            gamePlayMock.Verify(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()), Times.Once);
            gamePlayMock.Verify(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()), Times.Never);

            var diff = cmd.Start - now - finishTime;
            Assert.IsTrue(diff <= TimeSpan.FromSeconds(3)); // will fail while debugging because of date time now
            Assert.IsTrue(diff >= TimeSpan.FromSeconds(2));
        }

        [TestCase(false, BuildErrorType.NoSpaceInQueue)] // test one village with build action returns no erros
        [TestCase(true, BuildErrorType.NoSpaceInQueue)] // test one village with build action returns no space in queue error
        //[TestCase(true, BuildErrorType.NotEnoughResources)]
        public void Test_One_Village_Rescheduling_Next_Execution_Time(bool noSpaceInQueueError, BuildErrorType buildErrorType)
        {
            var testVillage = PrepareTestOneVillage(isBuildingFeatureOn: true);
            var resultVillage = _serviceProviderBuilder.Mapper.Map<Village>(testVillage);
            var finishTime = TimeSpan.FromMinutes(30);
            resultVillage.Dorf1BuildTimeLeft = finishTime;
            resultVillage.CanBuild = true;
            resultVillage.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage.Warehourse = 800;
            resultVillage.Granary = 800;

            var infoResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village> { resultVillage })
                .Build();

            var actionProviderMock = new Mock<IActionProvider>();
            actionProviderMock.Setup(x => x.GetBuildActions(resultVillage, It.IsAny<IEnumerable<BuildingModel>>()))
                .Returns(Task.FromResult((true, (IEnumerable<BuildAction>)new List<BuildAction> {
                    new BuildAction
                    {
                        Village = resultVillage,
                        BuildingId = "test",
                        BuildingSlot = "test",
                        Level = 1
                    }
                })));
            _serviceProviderBuilder.WithService(actionProviderMock.Object);

            var buildActionResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village>
                {
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CoordinateX = resultVillage.CoordinateX)
                        .With(y => y.CoordinateY = resultVillage.CoordinateY)
                        .With(y => y.CanBuild = false)
                        .With(y => y.Dorf1BuildTimeLeft = TimeSpan.FromMinutes(5))
                        .Build()
                })
                .With(x => x.Errors = noSpaceInQueueError 
                    ? new List<ScenarioError>
                        {
                            Builder<BuildScenarioError>.CreateNew()
                                .With(y => y.BuildErrorType = buildErrorType)
                                .With(y => y.Village = resultVillage)
                                .Build()
                        }
                    : new List<ScenarioError>())
                .Build();

            var gamePlayMock = new Mock<IGameplayClient>();
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()))
                .Returns(Task.FromResult(infoResult));
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()))
                .Returns(Task.FromResult(buildActionResult));
            _serviceProviderBuilder.WithService(gamePlayMock.Object);

            var cmd = new FakeCommand();
            var cmdMock = new Mock<ICommandFactory>();
            cmdMock.Setup(x => x.GetQueueableCommand(nameof(BuildCommand), It.IsAny<long>()))
                .Returns(cmd);
            _serviceProviderBuilder.WithService(cmdMock.Object);

            var job = new BuildingPlanExecutionJob(_serviceProviderBuilder.Build());
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET, true),
                JobType = typeof(BuildingPlanExecutionJob)
            };

            var now = DateTimeOffset.Now;
            Assert.DoesNotThrowAsync(async () => await job.Execute(data));

            _serviceProviderBuilder.LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            gamePlayMock.Verify(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()), Times.Once);

            var diff = cmd.Start - now - TimeSpan.FromMinutes(5);
            Assert.IsTrue(diff <= TimeSpan.FromSeconds(3)); // will fail while debugging because of date time now
            Assert.IsTrue(diff >= TimeSpan.FromSeconds(2));
        }

        [TestCase(BuildErrorType.NoSpaceInQueue)]
        [TestCase(BuildErrorType.NotEnoughResources)]
        public async Task Test_Three_Villages_Build_Action_Returns_Error(BuildErrorType errorType)
        {
            var testVillage = GetVillage(isBuildingFeatureOn: true);
            var resultVillage = _serviceProviderBuilder.Mapper.Map<Village>(testVillage);
            var finishTime = TimeSpan.FromMinutes(30);
            resultVillage.Dorf1BuildTimeLeft = finishTime;
            resultVillage.CanBuild = true;
            resultVillage.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage.Warehourse = 800;
            resultVillage.Granary = 800;

            var testVillage2 = GetVillage(isBuildingFeatureOn: true);
            var resultVillage2 = _serviceProviderBuilder.Mapper.Map<Village>(testVillage2);
            var finishTime2 = TimeSpan.FromMinutes(45);
            resultVillage2.Dorf1BuildTimeLeft = finishTime2;
            resultVillage2.CanBuild = true;
            resultVillage2.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage2.Warehourse = 800;
            resultVillage2.Granary = 800;

            var testVillage3 = GetVillage(isBuildingFeatureOn: false);
            resultVillage2.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage2.Warehourse = 800;
            resultVillage2.Granary = 800;

            var allVillages = new List<VillageModel> { testVillage, testVillage2, testVillage3 };
            var villageRepo = new Mock<IVillageRepository>();
            villageRepo.Setup(x => x.GetVillages(_serviceProviderBuilder.TravianUser.UserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)allVillages));
            villageRepo.Setup(x => x.GetVillage(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int x, int y) =>
                {
                    return Task.FromResult(allVillages.FirstOrDefault(z => z.CoordinateX == x && z.CoordinateY == y));
                });
            _serviceProviderBuilder.WithService(villageRepo.Object);

            var infoResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village> { resultVillage2 })
                .Build();

            var actionProviderMock = new Mock<IActionProvider>();
            actionProviderMock.Setup(x => x.GetBuildActions(resultVillage, It.IsAny<IEnumerable<BuildingModel>>()))
                .Returns(Task.FromResult((true, (IEnumerable<BuildAction>)new List<BuildAction> {
                    new BuildAction
                    {
                        Village = resultVillage,
                        BuildingId = "test",
                        BuildingSlot = "test",
                        Level = 1
                    }
                })));
            actionProviderMock.Setup(x => x.GetBuildActions(resultVillage2, It.IsAny<IEnumerable<BuildingModel>>()))
                .Returns(Task.FromResult((true, (IEnumerable<BuildAction>)new List<BuildAction> {
                    new BuildAction
                    {
                        Village = resultVillage2,
                        BuildingId = "test",
                        BuildingSlot = "test",
                        Level = 1
                    }
                })));
            _serviceProviderBuilder.WithService(actionProviderMock.Object);

            var buildActionResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village>
                {
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CanBuild = false)
                        .With(y => y.Dorf1BuildTimeLeft = TimeSpan.FromMinutes(2))
                        .With(y => y.CoordinateX = resultVillage.CoordinateX)
                        .With(y => y.CoordinateY = resultVillage.CoordinateY)
                        .With(y => y.Warehourse = 800)
                        .With(y => y.Granary = 800)
                        .With(y => y.Resources = Builder<Resources>.CreateNew().Build())
                        .With(y => y.ResourcesProduction = Builder<Resources>.CreateNew().Build())
                        .Build(),
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CanBuild = false)
                        .With(y => y.Dorf1BuildTimeLeft = TimeSpan.FromMinutes(5))
                        .With(y => y.CoordinateX = resultVillage2.CoordinateX)
                        .With(y => y.CoordinateY = resultVillage2.CoordinateY)
                        .Build()
                })
                .With(x => x.Errors = new List<ScenarioError>
                        {
                            Builder<BuildScenarioError>
                                .CreateNew()
                                .With(y => y.BuildErrorType = errorType)
                                .With(y => y.Village = resultVillage)
                                .Build()
                        })
                .Build();

            var gamePlayMock = new Mock<IGameplayClient>();
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()))
                .Returns(Task.FromResult(infoResult));
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()))
                .Returns(Task.FromResult(buildActionResult));
            _serviceProviderBuilder.WithService(gamePlayMock.Object);

            var cmd = new FakeCommand();
            var cmdMock = new Mock<ICommandFactory>();
            cmdMock.Setup(x => x.GetQueueableCommand(nameof(BuildCommand), It.IsAny<long>()))
                .Returns(cmd);
            _serviceProviderBuilder.WithService(cmdMock.Object);

            var job = new BuildingPlanExecutionJob(_serviceProviderBuilder.Build());
            var travianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET, true);
            var data = new JobExecutionData
            {
                TravianUser = travianUser,
                JobType = typeof(BuildingPlanExecutionJob)
            };

            var now = DateTimeOffset.Now;
            Assert.DoesNotThrowAsync(async () => await job.Execute(data));

            _serviceProviderBuilder.LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            gamePlayMock.Verify(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()), Times.Once);

            var expected = errorType == BuildErrorType.NoSpaceInQueue
                ? TimeSpan.FromMinutes(2)
                : Calculator.CalculateTimeForUnit(
                    resultVillage,
                    _serviceProviderBuilder.Mapper.Map<Village>(testVillage3), 
                    await _serviceProviderBuilder.UnitRepository.GetTrader(travianUser.PlayerData.Tribe));

            var diff = cmd.Start - now - TimeSpan.FromMinutes(2);
            Assert.IsTrue(diff <= TimeSpan.FromSeconds(3)); // will fail while debugging because of date time now
            Assert.IsTrue(diff >= TimeSpan.FromSeconds(2));

            buildActionResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village>
                {
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CanBuild = true)
                        .With(y => y.CoordinateX = resultVillage.CoordinateX)
                        .With(y => y.CoordinateY = resultVillage.CoordinateY)
                        .Build(),
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CanBuild = false)
                        .With(y => y.Dorf1BuildTimeLeft = TimeSpan.FromMinutes(5))
                        .With(y => y.CoordinateX = resultVillage2.CoordinateX)
                        .With(y => y.CoordinateY = resultVillage2.CoordinateY)
                        .Build()
                })
                .With(x => x.Errors = new List<ScenarioError>
                        {
                            Builder<BuildScenarioError>
                                .CreateNew()
                                .With(y => y.BuildErrorType = errorType)
                                .With(y => y.Village = resultVillage)
                                .Build()
                        })
                .Build();

            gamePlayMock = new Mock<IGameplayClient>();
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()))
                .Returns(Task.FromResult(infoResult));
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()))
                .Returns(Task.FromResult(buildActionResult));
            _serviceProviderBuilder.WithService(gamePlayMock.Object);

            job = new BuildingPlanExecutionJob(_serviceProviderBuilder.Build());
            now = DateTimeOffset.Now;
            Assert.DoesNotThrowAsync(async () => await job.Execute(data));

            _serviceProviderBuilder.LoggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Never);

            gamePlayMock.Verify(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()), Times.Once);

            diff = cmd.Start - now;
            Assert.IsTrue(diff <= TimeSpan.FromSeconds(3)); // will fail while debugging because of date time now
            Assert.IsTrue(diff >= TimeSpan.FromSeconds(2));
        }

        private AbstractJob PrepareOneVillageTest(BaseScenarioResult buildScenarioResult, BaseScenarioResult infoScenarioResult)
        {
            var testVillage = PrepareTestOneVillage(isBuildingFeatureOn: true);
            var resultVillage = _serviceProviderBuilder.Mapper.Map<Village>(testVillage);
            var finishTime = TimeSpan.FromMinutes(30);
            resultVillage.Dorf1BuildTimeLeft = finishTime;
            resultVillage.CanBuild = true;
            resultVillage.Resources = new Resources
            {
                Lumber = 100,
                Clay = 100,
                Iron = 100,
                Crop = 100
            };
            resultVillage.Warehourse = 800;
            resultVillage.Granary = 800;

            var infoResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village> { resultVillage })
                .Build();

            var actionProviderMock = new Mock<IActionProvider>();
            actionProviderMock.Setup(x => x.GetBuildActions(resultVillage, It.IsAny<IEnumerable<BuildingModel>>()))
                .Returns(Task.FromResult((true, (IEnumerable<BuildAction>)new List<BuildAction> {
                    new BuildAction
                    {
                        Village = resultVillage,
                        BuildingId = "test",
                        BuildingSlot = "test",
                        Level = 1
                    }
                })));
            _serviceProviderBuilder.WithService(actionProviderMock.Object);

            var buildActionResult = Builder<BaseScenarioResult>.CreateNew()
                .With(x => x.Success = true)
                .With(x => x.Villages = new List<Village>
                {
                    Builder<Village>
                        .CreateNew()
                        .With(y => y.CanBuild = false)
                        .With(y => y.Dorf1BuildTimeLeft = TimeSpan.FromMinutes(5))
                        .Build()
                })
                .Build();

            var gamePlayMock = new Mock<IGameplayClient>();
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<GameAction>>()))
                .Returns(Task.FromResult(infoResult));
            gamePlayMock.Setup(x => x.ExecuteActions(It.IsAny<TravianUser>(), It.IsAny<IEnumerable<BuildAction>>()))
                .Returns(Task.FromResult(buildActionResult));
            _serviceProviderBuilder.WithService(gamePlayMock.Object);

            var cmd = new FakeCommand();
            var cmdMock = new Mock<ICommandFactory>();
            cmdMock.Setup(x => x.GetQueueableCommand(nameof(BuildCommand), It.IsAny<long>()))
                .Returns(cmd);
            _serviceProviderBuilder.WithService(cmdMock.Object);

            var job = new BuildingPlanExecutionJob(_serviceProviderBuilder.Build());
            var data = new JobExecutionData
            {
                TravianUser = FakeDataProvider.GetUser(PlayerStatus.ALL_QUIET, true),
                JobType = typeof(BuildingPlanExecutionJob)
            };

            return job;
        }

        private VillageModel GetVillage(bool isBuildingFeatureOn = false, bool isWaitingForResources = false, DateTimeOffset? nextExecutionTime = null)
        {
            var rnd = new Random();
            return Builder<VillageModel>.CreateNew()
                .With(x => x.PlayerName = _serviceProviderBuilder.TravianUser.UserName)
                .With(x => x.IsBuildFeatureOn = isBuildingFeatureOn)
                .With(x => x.IsWaitingForResources = isWaitingForResources)
                .With(x => x.NextBuildingPlanExecutionTime = nextExecutionTime)
                .With(x => x.CoordinateX = rnd.Next(200) - 200)
                .With(x => x.CoordinateY = rnd.Next(200) - 200)
                .Build();
        }

        private VillageModel PrepareTestOneVillage(bool isBuildingFeatureOn = true, bool isWaitingForResources = false, DateTimeOffset? nextExecutionTime = null)
        {
            var testVillage = GetVillage(isBuildingFeatureOn, isWaitingForResources, nextExecutionTime);
            var allVillages = new List<VillageModel> { testVillage };
            var villageRepo = new Mock<IVillageRepository>();
            villageRepo.Setup(x => x.GetVillages(_serviceProviderBuilder.TravianUser.UserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)allVillages));
            villageRepo.Setup(x => x.GetVillage(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int x, int y) =>
                {
                    return Task.FromResult(allVillages.FirstOrDefault(z => z.CoordinateX == x && z.CoordinateY == y));
                });
            _serviceProviderBuilder.WithService(villageRepo.Object);

            return testVillage;
        }

        private List<VillageModel> PrepareTestTwoVillages(bool isBuildingFeatureOn = true, bool isWaitingForResources = false, DateTimeOffset? nextExecutionTime = null)
        {
            var testVillage = GetVillage(isBuildingFeatureOn, isWaitingForResources, nextExecutionTime);
            var testVillage2 = GetVillage(isBuildingFeatureOn, isWaitingForResources, nextExecutionTime);
            var allVillages = new List<VillageModel> { testVillage, testVillage2 };
            var villageRepo = new Mock<IVillageRepository>();
            villageRepo.Setup(x => x.GetVillages(_serviceProviderBuilder.TravianUser.UserName))
                .Returns(Task.FromResult((IEnumerable<VillageModel>)allVillages));
            villageRepo.Setup(x => x.GetVillage(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int x, int y) =>
                {
                    return Task.FromResult(allVillages.FirstOrDefault(z => z.CoordinateX == x && z.CoordinateY == y));
                });
            _serviceProviderBuilder.WithService(villageRepo.Object);

            return allVillages;
        }
    }
}
