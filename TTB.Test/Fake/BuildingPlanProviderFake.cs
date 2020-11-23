using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using TTB.DAL.Models;
using TTB.DAL.Repository;

namespace TTB.Test.Fake
{
    public class BuildingPlanProviderFake
    {
        private readonly List<BuildingPlanModel> _buildingPlans;
        private readonly IBuildingPlanRepository _buildingPlanRepository;
        private static readonly object _locker = new object();

        private static BuildingPlanProviderFake _instance;

        private BuildingPlanProviderFake()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\buildingPlans.json");
            var reader = new StreamReader(filepath);
            var json = reader.ReadToEnd();
            _buildingPlans = JsonConvert.DeserializeObject<List<BuildingPlanModel>>(json);

            var mock = new Mock<IBuildingPlanRepository>();
            mock.Setup(x => x.GetBuildingPlan(It.IsAny<string>()))
                    .Returns(Task.FromResult(_buildingPlans.FirstOrDefault(x => x.Name == "Default")));

            _buildingPlanRepository = mock.Object;
        }

        public static IBuildingPlanRepository GetBuildingRepository()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new BuildingPlanProviderFake();
                }
            }

            return _instance._buildingPlanRepository;
        }
    }
}
