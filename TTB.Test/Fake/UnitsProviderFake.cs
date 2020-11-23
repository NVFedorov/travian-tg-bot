using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;
using TTB.DAL.Repository;

namespace TTB.Test.Fake
{
    public class UnitsProviderFake
    {
        private readonly List<UnitModel> _units;
        private readonly IUnitRepository _unitRepository;

        public UnitsProviderFake()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\units.json");
            var reader = new StreamReader(filepath);
            var json = reader.ReadToEnd();
            _units = JsonConvert.DeserializeObject<List<UnitModel>>(json);

            var mock = new Mock<IUnitRepository>();
            mock.Setup(x => x.GetOffenceUnits(It.IsAny<Tribe>()))
                .Returns<Tribe>(tribe => Task.FromResult(GetOffenceUnits(tribe)));
            mock.Setup(x => x.GetDeffenceUnits(It.IsAny<Tribe>()))
                .Returns<Tribe>(tribe => Task.FromResult(GetDeffenceUnits(tribe)));
            mock.Setup(x => x.GetUnit(It.IsAny<string>(), It.IsAny<Tribe>()))
                .Returns<string, Tribe>((name, tribe) => Task.FromResult(GetUnit(name, tribe)));
            mock.Setup(x => x.GetUnitsByTribe(It.IsAny<Tribe>()))
                .Returns<Tribe>(tribe => Task.FromResult(GetUnitsByTribe(tribe)));
            mock.Setup(x => x.GetTrader(It.IsAny<Tribe>()))
                .Returns<Tribe>(tribe => Task.FromResult(GetUnit("trader", tribe)));

            _unitRepository = mock.Object;
        }

        public IUnitRepository GetUnitRepository()
        {
            return _unitRepository;
        }

        private IEnumerable<UnitModel> GetOffenceUnits(Tribe tribe)
        {
            return _units
                    .Where(x => x.Tribe == tribe &&
                        (x.UnitType == UnitType.FOOT_TROOPS || x.UnitType == UnitType.CAVALRY || x.UnitType == UnitType.WAR_MACHINE) &&
                        (x.Attack > x.DeffenceAgainstCavalry || x.Attack > x.DeffenceAgainstInfantry || x.UnitType == UnitType.WAR_MACHINE));
        }

        private IEnumerable<UnitModel> GetDeffenceUnits(Tribe tribe)
        {
            return _units
                    .Where(x => x.Tribe == tribe &&
                        (x.UnitType == UnitType.FOOT_TROOPS || x.UnitType == UnitType.CAVALRY) &&
                        (x.Attack < x.DeffenceAgainstCavalry || x.Attack < x.DeffenceAgainstInfantry));
        }

        private UnitModel GetUnit(string name, Tribe tribe)
        {
            return _units
                    .FirstOrDefault(x => x.Tribe == tribe && x.Name == name);
        }

        private IEnumerable<UnitModel> GetUnitsByTribe(Tribe tribe)
        {
            return _units.Where(x => x.Tribe == tribe);
        }
    }
}
