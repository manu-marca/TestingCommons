using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
namespace TestingCommons.MongoDb
{
    public class SerializationProvider : IBsonSerializationProvider
    {

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(decimal)) return new DecimalSerializer(BsonType.Decimal128);
            if (type == typeof(decimal?)) return new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128));
            if (type == typeof(DateTimeOffset)) return new DateTimeOffsetSerializer(BsonType.String);
            if (type == typeof(DateTimeOffset?)) return new NullableSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String));
            if (type == typeof(Guid)) return new GuidSerializer(GuidRepresentation.Standard);
            return null;
        }
    }
}
