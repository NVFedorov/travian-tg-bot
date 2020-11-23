using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TTB.DAL.Repository
{
    public interface ILogRepository<T>
    {
        Task<List<T>> GetAll();
        Task<List<T>> GetLast(int numberOfRecord, string filters = "");
        Task<List<T>> GetLast(DateTime start, string filter = "");
    }
}
