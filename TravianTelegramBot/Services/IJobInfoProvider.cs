using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravianTelegramBot.ViewModels;
using TTB.DAL.Models;

namespace TravianTelegramBot.Services
{
    public interface IJobInfoProvider
    {
        Task<IEnumerable<JobInfoViewModel>> GetJobsDetailsForPlayer(TravianUser player);
    }
}
