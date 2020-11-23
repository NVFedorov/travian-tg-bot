using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository
{
    public interface INotificationRepository
    {
        Task<List<IncomingAttackNotificationModel>> GetIncomingAttacksAndScans(string userName);
    }
}
