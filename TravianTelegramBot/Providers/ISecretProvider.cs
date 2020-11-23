using System.Threading.Tasks;
using TTB.DAL.Models;

namespace TravianTelegramBot.Providers
{
    public interface ISecretProvider
    {
        Task<SecretModel> CheckSecretCode(string code);
    }
}
