using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using TTB.DAL.Models;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.PlayerModels.Enums;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.Test.Fake
{
    public static class FakeDataProvider
    {
        public static readonly DateTimeOffset ExistingAttackDateTime = DateTimeOffset.UtcNow.AddMinutes(1);
        public static readonly DateTimeOffset NewAttackDateTime = DateTimeOffset.UtcNow.AddMinutes(5);
        public static readonly string TravianUserName = "TestTravianUser";
        public static readonly string TestVillageName = "test_village_under_attack";
        public static readonly string BotUserName = "TestBotUser";
        public static readonly string TimeZone = "UTC+2";

        public static List<VillageModel> GetVillagesFromDatabase(bool attacked)
        {
            var builder = Builder<VillageModel>.CreateListOfSize(2)
                    .TheFirst(1)
                    .With(y => y.VillageName = TestVillageName)
                    .All()
                    .With(y => y.Alliance = "TestAlliance")
                    .With(y => y.CoordinateX = new Random().Next(401) - 200)
                    .With(y => y.CoordinateY = new Random().Next(401) - 200);

            if (attacked)
            {
                builder = builder
                    .TheFirst(1)
                    .With(y => y.VillageName = TestVillageName)
                    .With(y => y.Attacks = Builder<AttackModel>
                        .CreateListOfSize(1)
                        .All()
                        .With(z => z.VillageName = TestVillageName)
                        .With(z => z.DateTime = ExistingAttackDateTime)
                        .With(z => z.IntruderVillageUrl = "test.coma")
                        .Build()
                        .ToList());
            }

            return builder.Build().ToList();
        }

        public static TravianUser GetUser(PlayerStatus status, bool withPlayerData = true)
        {
            var builder = Builder<TravianUser>.CreateNew()
                .With(x => x.UserName = TravianUserName)
                .With(x => x.BotUserName = BotUserName)
                .With(x => x.Url = "http://valid-schema.why")
                .With(x => x.IsActive = true);

            if (withPlayerData)
                builder = builder
                .With(x => x.ExecutionContext = Builder<ExecutionContextModel>.CreateNew().Build())
                .With(x => x.PlayerData = Builder<PlayerDataModel>.CreateNew()
                    .With(y => y.UserName = TravianUserName)
                    .With(y => y.Status = status)
                    .With(y => y.TimeZone = TimeZone)
                    .With(y => y.Tribe = DAL.Models.GameModels.Enums.Tribe.GAUL)
                    .Build());

            return builder.Build();
        }
    }
}
