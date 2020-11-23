using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Repository;

namespace TTB.Test.Fake
{
    public class BuildingsProviderFake
    {
        private readonly List<BuildingModel> _buildings;
        private readonly IBuildingRepository _buildingRepository;
        private static readonly object _locker = new object();

        private static BuildingsProviderFake _instance;

        private BuildingsProviderFake()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\buildings.json");
            var reader = new StreamReader(filepath);
            var json = reader.ReadToEnd();
            _buildings = JsonConvert.DeserializeObject<List<BuildingModel>>(json);

            var mock = new Mock<IBuildingRepository>();
            mock.Setup(x => x.GetAllBuildings())
                    .Returns(Task.FromResult(GetAllBuildings()));

            _buildingRepository = mock.Object;
        }

        public static IBuildingRepository GetBuildingRepository()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new BuildingsProviderFake();
                }
            }

            return _instance._buildingRepository;
        }

        private IEnumerable<BuildingModel> GetAllBuildings()
        {
            return _buildings;
        }
    }
}
