using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using TravianTelegramBot.Mapping;
using TravianTelegramBot.Providers;
using TravianTelegramBot.Providers.Impl;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;
using TTB.DAL.Models.ScenarioModels.Enums;
using TTB.DAL.Repository;
using TTB.Gameplay.Models.ContextModels;
using TTB.Gameplay.Models.ContextModels.Actions;
using TTB.Test.Fake;

namespace TTB.Test.ServicesTests
{
    [TestFixture]
    public class ActionProviderTests
    {
        private IUnitRepository _unitRepo;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _unitRepo = new UnitsProviderFake().GetUnitRepository();

            _mapper = new Mapper(
                new MapperConfiguration(
                configure =>
                {
                    configure.AddProfile<MappingProfile>();
                })
            );

        }

        [Test]
        public async Task Test_Get_Actions_For_Player_When_Only_Capital_Is_Under_Attack()
        {
            var villages = Builder<VillageModel>.CreateListOfSize(3)
                .TheFirst(1)
                .With(x => x.CoordinateX = 0)
                .With(x => x.CoordinateY = 0)
                .With(x => x.IsCapital = true)
                .With(x => x.IsSaveResourcesFeatureOn = true)
                .With(x => x.IsSaveTroopsFeatureOn = true)
                .With(x => x.Attacks = Builder<AttackModel>.CreateListOfSize(1).All().With(y => y.DateTime = DateTimeOffset.UtcNow.AddMinutes(1)).Build().ToList())
                .Build();

            var user = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK);
            var actionProvider = BuildAction(villages);
            var actions = (await actionProvider.GetActionsForPlayer(user)).ToList();
            Assert.AreEqual(2, actions.Count);
            Assert.AreEqual(GameActionType.SEND_ARMY, actions[0].Action);
            Assert.AreEqual(GameActionType.TRAIN_ARMY, actions[1].Action);
        }

        [Test]
        public async Task Test_Get_Actions_For_Player_When_Only_Capital_Is_Under_Attack_And_Features_Off()
        {
            var villages = Builder<VillageModel>.CreateListOfSize(3)
                .TheFirst(1)
                .With(x => x.CoordinateX = 0)
                .With(x => x.CoordinateY = 0)
                .With(x => x.IsCapital = true)
                .With(x => x.Attacks = Builder<AttackModel>.CreateListOfSize(1).All().With(y => y.DateTime = DateTimeOffset.UtcNow.AddMinutes(1)).Build().ToList())
                .Build();

            var user = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK);
            var actionProvider = BuildAction(villages);
            var actions = (await actionProvider.GetActionsForPlayer(user)).ToList();
            Assert.AreEqual(0, actions.Count);
        }

        [Test]
        public async Task Test_Get_Actions_For_Player_When_Capital_And_Another_Village_Are_Under_Attack()
        {
            var swordsman = await _unitRepo.GetUnit("swordsman", DAL.Models.GameModels.Enums.Tribe.GAUL);
            var theutates_thunder = await _unitRepo.GetUnit("theutates_thunder", DAL.Models.GameModels.Enums.Tribe.GAUL);
            var trebuchet = await _unitRepo.GetUnit("trebuchet", DAL.Models.GameModels.Enums.Tribe.GAUL);
            var phalang = await _unitRepo.GetUnit("phalang", DAL.Models.GameModels.Enums.Tribe.GAUL);

            var villages = Builder<VillageModel>
                            .CreateListOfSize(3)
                            .TheFirst(1)
                            .With(y => y.VillageName = "uservillage1")
                            .With(y => y.IsCapital = true)
                            .With(y => y.CoordinateX = 0)
                            .With(y => y.CoordinateY = 0)
                            .With(y => y.Types = new List<VillageType> { VillageType.OFFENCE })
                            .With(y => y.IsSaveResourcesFeatureOn = true)
                            .With(y => y.IsSaveTroopsFeatureOn = true)
                            .With(y => y.PreferableUnits = new List<string> { "swordsman", "theutates_thunder", "trebuchet" })
                            .With(y => y.Attacks = Builder<AttackModel>
                                .CreateListOfSize(2)
                                .TheFirst(1)
                                .With(x => x.DateTime = DateTimeOffset.UtcNow.AddMinutes(10))
                                .TheNext(1)
                                .With(x => x.DateTime = DateTimeOffset.UtcNow.AddMinutes(200))
                                .Build()
                                .ToList())
                            .TheNext(1)
                            .With(y => y.VillageName = "uservillage2")
                            .With(y => y.CoordinateX = -2)
                            .With(y => y.CoordinateY = -3)
                            .With(y => y.IsCapital = false)
                            .With(y => y.Types = new List<VillageType> { VillageType.DEFFENCE, VillageType.RESOURCES })
                            .TheLast(1)
                            .With(y => y.VillageName = "uservillage3")
                            .With(y => y.CoordinateX = -5)
                            .With(y => y.CoordinateY = -2)
                            .With(y => y.IsCapital = false)
                            .With(y => y.IsSaveResourcesFeatureOn = true)
                            .With(y => y.IsSaveTroopsFeatureOn = true)
                            .With(y => y.Types = new List<VillageType> { VillageType.DEFFENCE, VillageType.RESOURCES })
                            .With(y => y.Attacks = Builder<AttackModel>
                                .CreateListOfSize(2)
                                .TheFirst(1)
                                .With(x => x.DateTime = DateTimeOffset.UtcNow.AddMinutes(10))
                                .TheNext(1)
                                .With(x => x.DateTime = DateTimeOffset.UtcNow.AddMinutes(30))
                                .Build()
                                .ToList())
                            .With(y => y.PreferableUnits = new List<string> { "phalang" })
                            .Build();
            
            var user = FakeDataProvider.GetUser(PlayerStatus.UNDER_ATTACK);
            var actionProvider = BuildAction(villages);
            var actions = (await actionProvider.GetActionsForPlayer(user)).ToList();

            Assert.AreEqual(5, actions.Count);

            Assert.AreEqual(GameActionType.SEND_ARMY, actions[0].Action);
            Assert.AreEqual(GameActionType.TRAIN_ARMY, actions[1].Action);
            Assert.AreEqual(GameActionType.SEND_RESOURCES, actions[2].Action);
            Assert.AreEqual(GameActionType.TRAIN_ARMY, actions[3].Action);
            Assert.AreEqual(GameActionType.SEND_ARMY, actions[4].Action);


            Assert.IsNotNull((actions[1] as TrainArmyAction).UnitsToTrain);
            Assert.IsTrue((actions[1] as TrainArmyAction).UnitsToTrain.Any());
            Assert.AreEqual(typeof(SendResourcesAction), actions[2].GetType());
            Assert.AreEqual((actions[2] as SendResourcesAction).To.Name, "uservillage2");
            Assert.IsNotNull((actions[3] as TrainArmyAction).UnitsToTrain);
            Assert.IsTrue((actions[3] as TrainArmyAction).UnitsToTrain.Any());

            Assert.AreEqual(3, (actions[1] as TrainArmyAction).UnitsToTrain.Count);
            Assert.AreEqual(swordsman.LocalizedNameRu, (actions[1] as TrainArmyAction).UnitsToTrain.Keys.ToList()[0]);
            Assert.AreEqual(theutates_thunder.LocalizedNameRu, (actions[1] as TrainArmyAction).UnitsToTrain.Keys.ToList()[1]);
            Assert.AreEqual(trebuchet.LocalizedNameRu, (actions[1] as TrainArmyAction).UnitsToTrain.Keys.ToList()[2]);
            Assert.AreEqual(1, (actions[3] as TrainArmyAction).UnitsToTrain.Count);
            Assert.AreEqual(phalang.LocalizedNameRu, (actions[3] as TrainArmyAction).UnitsToTrain.Keys.ToList()[0]);
        }

        private IActionProvider BuildAction(IEnumerable<VillageModel> villages)
        {
            var villageRepoMock = new Mock<IVillageRepository>();
            villageRepoMock.Setup(x => x.GetVillages(FakeDataProvider.TravianUserName))
                .Returns(Task.FromResult(villages));

            var buildingRepo = BuildingsProviderFake.GetBuildingRepository();

            var buildingPlanRepo = BuildingPlanProviderFake.GetBuildingRepository();

            return new ActionProvider(_unitRepo, villageRepoMock.Object, buildingRepo, buildingPlanRepo, _mapper);
        }
    }
}
