using MassTransit;

namespace TestingCommons.RabbitMq
{
    public class RabbitMqClient : IDisposable
    {
        private readonly IBusControl _bus;

        public RabbitMqClient(RabbitMqConfig rabbitMqConfiguration)
        {
            _bus = Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.ConfigureRabbitMq(rabbitMqConfiguration);
            });
            Start();
        }

        public RabbitMqClient(IBusControl busControl)
        {
            _bus = busControl;
            Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public Guid PublishMessage<T>(T message, int redeliveryCount = 0) where T : class
        {
            var publisher = _bus.GetPublishSendEndpoint<T>().Result;
            var requestId = NewId.NextGuid();

            publisher.Send(message, context =>
            {
                context.RequestId = requestId;
                if (redeliveryCount > 0)
                {
                    context.Headers.Set(MessageHeaders.RedeliveryCount, redeliveryCount);
                }
            })
                .Wait(TimeSpan.FromSeconds(1));

            return requestId;
        }

        public void Stop()
        {
            _bus.Stop(TimeSpan.FromMinutes(5));
        }

        private void Start()
        {
            var health = _bus.CheckHealth();
            if (health.Status == BusHealthStatus.Unhealthy)
            {
                if (health.Description == "Not ready: not started")
                {
                    _bus.Start(TimeSpan.FromSeconds(30));
                }
                else
                {
                    throw new Exception($"Status: {health.Description}");
                }
            }
        }
    }
}
