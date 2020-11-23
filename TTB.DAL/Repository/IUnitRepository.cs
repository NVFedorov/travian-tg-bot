using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;
using TTB.DAL.Models.GameModels.Enums;

namespace TTB.DAL.Repository
{
    public interface IUnitRepository : IKnowledgeRepository
    {
        Task<UnitModel> GetUnit(string name, Tribe tribe);
        Task<IEnumerable<UnitModel>> GetOffenceUnits(Tribe tribe);
        Task<IEnumerable<UnitModel>> GetDeffenceUnits(Tribe tribe);
        Task<UnitModel> GetScanUnit(Tribe tribe);
        Task<UnitModel> GetTrader(Tribe tribe);
        Task<IEnumerable<UnitModel>> GetUnitsByType(Tribe tribe, UnitType type);
        Task<IEnumerable<UnitModel>> GetUnitsByTribe(Tribe tribe);
        Task<IEnumerable<UnitModel>> GetAllUnits();
    }
}
