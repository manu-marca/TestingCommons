using MongoDB.Driver;

namespace TestingCommons.MongoDb
{
    public interface IMongoDbClientBase
    {
        IMongoDatabase GetDatabase();
    }
}