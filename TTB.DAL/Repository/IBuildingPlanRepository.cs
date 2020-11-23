using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TTB.DAL.Repository
{
    public interface IBuildingPlanRepository
    {
        Task<BuildingPlanModel> GetBuildingPlan(string id);
        Task<IEnumerable<BuildingPlanModel>> GetBuildingPlans(string botUserName);
        Task Insert(BuildingPlanModel item);
        Task<bool> Update(BuildingPlanModel item);
        Task<bool> Delete(string id);
    }
}
