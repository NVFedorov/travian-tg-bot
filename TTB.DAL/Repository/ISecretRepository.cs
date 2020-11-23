using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TTB.DAL.Repository
{
    public interface ISecretRepository
    {
        Task<SecretModel> GetByValue(string value);
    }
}
