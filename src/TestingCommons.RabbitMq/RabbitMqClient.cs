using MassTransit;

namespace TestingCommons.RabbitMq
{
    public class RabbitMqClient : IDisposable
    {
        private readonly IBusControl _bus;
        private readonly List<HostReceiveEndpointHandle> _receiveEndpointHandles = new List<HostReceiveEndpointHandle>();

        public RabbitMqClient(RabbitMqConfig rabbitMqConfiguration)
        {
            _bus = Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.ConfigureRabbitMq(rabbitMqConfiguration);
            });
            Start();
        }

        public RabbitMqClient(RabbitMqConfig rabbitMqConfiguration, 
            Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>> receiveEndpointConfigurations)
        {
            _bus = BusControl.InitializeWithConsumers(rabbitMqConfiguration, receiveEndpointConfigurations);
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

        public HostReceiveEndpointHandle ConsumeMessages<T>(string queueName, Func<T, Task> messageHandler, CancellationToken cancellationToken = default) 
            where T : class
        {
            var receiveEndpoint = _bus.ConnectReceiveEndpoint(queueName, endpoint =>
            {
                endpoint.Consumer(() => new MessageConsumer<T>(messageHandler));
            });

            _receiveEndpointHandles.Add(receiveEndpoint);
            return receiveEndpoint;
        }

        public HostReceiveEndpointHandle ConsumeMessages<T>(string queueName, Func<T, ConsumeContext<T>, Task> messageHandler, CancellationToken cancellationToken = default) 
            where T : class
        {
            var receiveEndpoint = _bus.ConnectReceiveEndpoint(queueName, endpoint =>
            {
                endpoint.Consumer(() => new ContextAwareMessageConsumer<T>(messageHandler));
            });

            _receiveEndpointHandles.Add(receiveEndpoint);
            return receiveEndpoint;
        }

        public HostReceiveEndpointHandle ConsumeMessages<T>(string queueName, IConsumer<T> consumer, CancellationToken cancellationToken = default) 
            where T : class
        {
            var receiveEndpoint = _bus.ConnectReceiveEndpoint(queueName, endpoint =>
            {
                endpoint.Consumer(() => consumer);
            });

            _receiveEndpointHandles.Add(receiveEndpoint);
            return receiveEndpoint;
        }

        public async Task WaitForMessages(TimeSpan timeout)
        {
            if (_receiveEndpointHandles.Any())
            {
                await Task.Delay(timeout);
            }
        }

        public async Task StopConsumers()
        {
            if (_receiveEndpointHandles.Any())
            {
                foreach (var handle in _receiveEndpointHandles)
                {
                    await handle.StopAsync();
                }
                _receiveEndpointHandles.Clear();
            }
        }

        public void Stop()
        {
            StopConsumers().GetAwaiter().GetResult();
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

    internal class MessageConsumer<T> : IConsumer<T> where T : class
    {
        private readonly Func<T, Task> _messageHandler;

        public MessageConsumer(Func<T, Task> messageHandler)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            await _messageHandler(context.Message);
        }
    }

    internal class ContextAwareMessageConsumer<T> : IConsumer<T> where T : class
    {
        private readonly Func<T, ConsumeContext<T>, Task> _messageHandler;

        public ContextAwareMessageConsumer(Func<T, ConsumeContext<T>, Task> messageHandler)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public async Task Consume(ConsumeContext<T> context)
        {
            await _messageHandler(context.Message, context);
        }
    }
}
