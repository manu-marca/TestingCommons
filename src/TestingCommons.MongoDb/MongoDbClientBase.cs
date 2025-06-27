using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace TestingCommons.MongoDb
{
    public abstract class MongoDbClientBase : IMongoDbClientBase
    {
        private MongoClient _client;
        private MongoDbConfiguration _mongoDbConfiguration;

        public static IMongoDatabase Database { get; internal set; }

        protected MongoDbClientBase(MongoDbConfiguration mongoSettings)
        {
            ConfigureBsonSerializer();
            InitializeDatabase(mongoSettings);
        }

        private void InitializeDatabase(MongoDbConfiguration mongoSettings)
        {
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreIfNullConvention(true),
                new IgnoreExtraElementsConvention(true)

            };
            ConventionRegistry.Register("default", conventionPack, t => true);

            var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoSettings.ConnectionString));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.RetryWrites = false;
            _client = new MongoClient(settings);
            Database = _client.GetDatabase(mongoSettings.Database);

            _mongoDbConfiguration = mongoSettings;
        }

        private static void ConfigureBsonSerializer()
        {
            //remove string representation of decimals
            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            //normalize dataTimeOffset representation to string
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset), new DateTimeOffsetSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(typeof(DateTimeOffset?), new NullableSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String)));
            //normalize guid representation to UUID standard
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public IMongoDatabase GetDatabase() => _client.GetDatabase(_mongoDbConfiguration.Database);
    }
}
