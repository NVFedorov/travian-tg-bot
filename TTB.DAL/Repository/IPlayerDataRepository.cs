using System.Threading.Tasks;
using TTB.DAL.Models.PlayerModels;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository
{
    public interface IPlayerDataRepository
    {
        Task<PlayerDataModel> GetPlayerDataForUser(string travianUserName);
        Task Insert(PlayerDataModel user);
        Task<bool> Update(PlayerDataModel item);
        Task<bool> Delete(string travianUserName);
    }
}
