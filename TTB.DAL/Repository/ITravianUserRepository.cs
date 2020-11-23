using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TTB.DAL.Repository
{
    public interface ITravianUserRepository
    {
        Task<TravianUser> GetUserByName(string userName, string botUserName);
        Task<IEnumerable<TravianUser>> GetUsersByBotUser(string botUserName);
        Task<TravianUser> GetActiveUser(string botUserName);
        Task Insert(TravianUser user);
        Task<bool> Update(TravianUser user);
        Task<bool> ReplacePlayerData(TravianUser user);
        Task<bool> ReplacePlayerDataVillages(TravianUser user);
        Task<bool> Update(IEnumerable<TravianUser> user);
        Task<bool> Delete(string userName, string botUserName);
    }
}
