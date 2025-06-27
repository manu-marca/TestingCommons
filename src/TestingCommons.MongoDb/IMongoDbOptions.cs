namespace TestingCommons.MongoDb
{
    public interface IMongoDbOptions
    {
        public string ConnectionString { get; }

        public string DatabaseName { get; }
    }
}
