using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTB.DAL.Models.ScenarioModels;

namespace TTB.DAL.Repository
{
    public interface IArmyEventRepository
    {
        Task<List<ArmyEventModel>> GetArmyEvents(string userName, DateTimeOffset dateTime);
        Task<List<ArmyEventModel>> GetNewEvents(string userName);
        Task<bool> RemoveOldArmyEvents(string userName);
        Task<bool> Update(ArmyEventModel item);
        Task<bool> Update(List<ArmyEventModel> items);
    }
}
