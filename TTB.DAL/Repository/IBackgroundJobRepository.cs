using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TTB.DAL.Repository
{
    public interface IBackgroundJobRepository
    {
        Task<BackgroundJobModel> GetBackgroundJobForUser(string botUserName);
        Task<bool> Update(BackgroundJobModel item);
        Task Insert(BackgroundJobModel item);
        Task<bool> Delete(BackgroundJobModel item);
    }
}
