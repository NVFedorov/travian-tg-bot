using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TTB.DAL.Models;

namespace TTB.DAL.Repository.Impl
{
    public class SecretReporitory : Repository<SecretModel>, ISecretRepository
    {
        private const string CollectionName = "managerSecrets";
        public SecretReporitory(IMongoDatabase database, ILogger<Repository<SecretModel>> logger) : base(database, CollectionName, logger)
        {
        }

        public async Task<SecretModel> GetByValue(string value)
        {
            var filter = Builders<SecretModel>.Filter.Eq("value", value);
            return (await base.Get(filter)).FirstOrDefault();
        }
    }
}
