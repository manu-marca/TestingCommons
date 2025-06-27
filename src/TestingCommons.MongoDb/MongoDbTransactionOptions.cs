using MongoDB.Driver;

namespace TestingCommons.MongoDb
{
    public class MongoDbTransactionOptions
    {
        public ReadConcernLevel ReadConcern { get; private set; }

        /// <summary>
        /// Accepted values: https://www.mongodb.com/docs/manual/reference/write-concern/#w-option
        /// </summary>
        private string WriteConcern { get; set; }

        public WriteConcern.WValue GetWriteConcernValue()
        {
            if (string.IsNullOrEmpty(WriteConcern))
                throw new ArgumentNullException(nameof(WriteConcern));
            return MongoDB.Driver.WriteConcern.WValue.Parse(WriteConcern);
        }

        public MongoDbTransactionOptions() { }
        public MongoDbTransactionOptions(string writeConcern)
        {
            WriteConcern = writeConcern;
        }
    }
}
