using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TTB.DAL.Repository
{
    public interface IKnowledgeRepository
    {
        Task UpdateCollection(string json);
    }
}
