using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository
{
    public interface IVillageRepository
    {
        Task<VillageModel> GetVillage(string villageId);
        Task<VillageModel> GetVillage(int x, int y);
        Task<IEnumerable<VillageModel>> GetVillages(string playerName);
        Task Insert(VillageModel model);
        Task<bool> Update(VillageModel model);
        Task<bool> Update(IEnumerable<VillageModel> model);
        Task<bool> UpdateWatchInfo(IEnumerable<VillageModel> model);
        Task<bool> UpdateInfos(IEnumerable<VillageModel> model);
        Task<bool> UpdateBaseInfos(IEnumerable<VillageModel> model);
        Task<bool> Delete(string name, int x, int y);
        Task<bool> DeleteVillages(string playerName);
    }
}
