namespace TestingCommons.RabbitMq
{
    public class RedeliverySettings
    {
        public int MaxAttempts { get; set; }
        public int DelayTimeMultiplier { get; set; }
        public int MaximumDelaySeconds { get; set; }
    }
}
