using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models.GameModels;

namespace TTB.DAL.Repository
{
    public interface IBuildingRepository : IKnowledgeRepository
    {
        Task<BuildingModel> GetBuilding(string buildingId);
        Task<IEnumerable<BuildingModel>> GetAllBuildings();
        Task Insert(BuildingModel item);
        Task<bool> Update(BuildingModel item);
        Task<bool> Delete(string buildingId);
    }
}
