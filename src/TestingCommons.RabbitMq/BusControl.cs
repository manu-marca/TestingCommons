using MassTransit;
using MassTransit.RabbitMqTransport;
using System.Net.Security;
using TestingCommons.RabbitMq;

namespace TestingCommons.RabbitMq
{
    public static class BusControl
    {
        public static IBusControl Initialize(RabbitMqConfig rabbitMqConfiguration, Action<IRabbitMqBusFactoryConfigurator> configureAction = null)
            => Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.ConfigureRabbitMq(rabbitMqConfiguration);
                configureAction?.Invoke(configure);
            });

        public static IBusControl InitializeWithConsumers(RabbitMqConfig rabbitMqConfiguration, 
            Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>> receiveEndpointConfigurations = null,
            Action<IRabbitMqBusFactoryConfigurator> additionalConfiguration = null)
            => Bus.Factory.CreateUsingRabbitMq(configure =>
            {
                configure.ConfigureRabbitMq(rabbitMqConfiguration);
                
                // Configure receive endpoints for consumers
                if (receiveEndpointConfigurations != null)
                {
                    foreach (var endpointConfig in receiveEndpointConfigurations)
                    {
                        configure.ReceiveEndpoint(endpointConfig.Key, endpointConfig.Value);
                    }
                }
                
                additionalConfiguration?.Invoke(configure);
            });

        public static void ConfigureRabbitMq(this IRabbitMqBusFactoryConfigurator configure, RabbitMqConfig rabbitMqConfiguration)
        {
            ConfigureHost(rabbitMqConfiguration, configure);
            configure.PrefetchCount = 8;
        }

        private static void ConfigureHost(RabbitMqConfig rabbitMqConfiguration,
            IRabbitMqBusFactoryConfigurator configure)
        {
            var virtualHost = string.IsNullOrEmpty(rabbitMqConfiguration.VirtualHost) ? "/" : rabbitMqConfiguration.VirtualHost;
            configure.Host(rabbitMqConfiguration.Hostname, rabbitMqConfiguration.Port, virtualHost, c =>
            {
                c.Username(rabbitMqConfiguration.Username);
                c.Password(rabbitMqConfiguration.Password);
                var nodeList = rabbitMqConfiguration.ClusterNodesList;
                if (nodeList.Length > 0)
                {
                    c.UseCluster(x =>
                    {
                        foreach (var node in nodeList)
                        {
                            x.Node(node);
                        }
                    });
                }

                if (rabbitMqConfiguration.IsTlsConnection)
                {
                    c.UseSsl(sc => ConfigureSsl(sc, rabbitMqConfiguration));
                }

                var heartbeat = rabbitMqConfiguration.HeartbeatInterval;
                if (heartbeat > 0)
                {
                    c.Heartbeat((ushort)heartbeat);
                }
            });
        }

        private static void ConfigureSsl(IRabbitMqSslConfigurator sc, RabbitMqConfig rabbitMqConfiguration)
        {
            if (rabbitMqConfiguration.UseTlsPolicy)
            {
                const SslPolicyErrors policy = SslPolicyErrors.RemoteCertificateChainErrors |
                                               SslPolicyErrors.RemoteCertificateNameMismatch |
                                               SslPolicyErrors.RemoteCertificateNotAvailable;
                sc.AllowPolicyErrors(policy);
            }
            sc.ServerName = rabbitMqConfiguration.Hostname;
            sc.Protocol = System.Security.Authentication.SslProtocols.Tls12;
            sc.UseCertificateAsAuthenticationIdentity = false;
        }
    }
}
