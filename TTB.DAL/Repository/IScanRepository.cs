using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository
{
    public interface IScanRepository
    {
        Task<List<ScanNotificationModel>> GetScans(string userName);
        Task<bool> Update(ScanNotificationModel scan);
        Task<bool> Update(IEnumerable<ScanNotificationModel> scans);
    }
}
