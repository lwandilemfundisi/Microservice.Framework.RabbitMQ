using Microservice.Framework.Common;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Reflection;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly ILogger<RabbitMqConnectionFactory> _log;
        private readonly IRabbitMqConfiguration _configuration;
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly Dictionary<Uri, ConnectionFactory> _connectionFactories = new Dictionary<Uri, ConnectionFactory>();

        public RabbitMqConnectionFactory(
            ILogger<RabbitMqConnectionFactory> log,
            IRabbitMqConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        public async Task<IRabbitConnection> CreateConnectionAsync(
            Uri uri, 
            CancellationToken cancellationToken)
        {
            var connectionFactory = await CreateConnectionFactoryAsync(uri, cancellationToken).ConfigureAwait(false);
            var connection = connectionFactory.CreateConnection();

            return new RabbitConnection(_log, _configuration.ModelsPrConnection, connection);
        }

        private async Task<ConnectionFactory> CreateConnectionFactoryAsync(Uri uri, CancellationToken cancellationToken)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                ConnectionFactory connectionFactory;
                if (_connectionFactories.TryGetValue(uri, out connectionFactory))
                {
                    return connectionFactory;
                }
                _log.LogInformation("Creating RabbitMQ connection factory to {0}", uri.Host);

                connectionFactory = new ConnectionFactory
                {
                    Uri = uri,
                    UseBackgroundThreadsForIO = true, // TODO: As soon as RabbitMQ supports async/await, set to false
                    TopologyRecoveryEnabled = true,
                    AutomaticRecoveryEnabled = true,
                    ClientProperties = new Dictionary<string, object>
                            {
                                { "microservice.framework.rabbitmq-version", typeof(RabbitMqConnectionFactory).GetTypeInfo().Assembly.GetName().Version.ToString() },
                                { "machine-name", Environment.MachineName },
                            },
                };

                _connectionFactories.Add(uri, connectionFactory);
                return connectionFactory;
            }
        }
    }
}
