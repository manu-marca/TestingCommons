using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingCommons.MongoDb
{
    public class MongoDbConfiguration
    {
        public string ConnectionString { get; init; }
        public string Database { get; init; }
        public bool EnableCommandTextLogging { get; init; }
        public bool EnableMongoDbConnectionDiagnostics { get; init; }
        public MongoDbMessageConsumeDelayConfiguration MessageConsumeDelay { get; init; } = new MongoDbMessageConsumeDelayConfiguration();
        public bool HasTransactionSupportEnabled { get; init; }
        public MongoDbTransactionOptions TransactionOptions { get; init; }
        public ReadPreferenceMode? ReadPreferenceMode { get; init; }
        /// <summary>
        /// The maximum staleness to allow when reading from secondaries.
        /// Min value is 90 seconds.
        /// </summary>
        public int MaxStalenessSeconds { get; init; }
    }
}
