namespace TestingCommons.MongoDb
{
    public class MongoDbMessageConsumeDelayConfiguration
    {
        public bool? Active { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public int? Steps { get; set; }
        public double? Jitter { get; set; }
    }
}
