using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TTB.DAL.Repository
{
    public abstract class KnowledgeRepository<T> : Repository<T>, IKnowledgeRepository
    {
        public KnowledgeRepository(IMongoDatabase database, string collectionName, ILogger<Repository<T>> logger) : base(database, collectionName, logger)
        {
        }

        public async Task UpdateCollection(string json)
        {
            await _collection.DeleteManyAsync(new BsonDocument());
            var document = BsonSerializer.Deserialize<IEnumerable<T>>(json);
            await _collection.InsertManyAsync(document);
        }
    }
}
