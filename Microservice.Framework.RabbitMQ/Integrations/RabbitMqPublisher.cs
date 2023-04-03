using Microservice.Framework.Common;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitMqPublisher : IDisposable, IRabbitMqPublisher
    {
        private readonly ILogger<RabbitMqPublisher> _log;
        private readonly IRabbitMqConnectionFactory _connectionFactory;
        private readonly IRabbitMqConfiguration _configuration;
        private readonly ITransientFaultHandler<IRabbitMqResilientStrategy> _transientFaultHandler;
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly Dictionary<Uri, IRabbitConnection> _connections = new Dictionary<Uri, IRabbitConnection>();

        public RabbitMqPublisher(
            ILogger<RabbitMqPublisher> log,
            IRabbitMqConnectionFactory connectionFactory,
            IRabbitMqConfiguration configuration,
            ITransientFaultHandler<IRabbitMqResilientStrategy> transientFaultHandler)
        {
            _log = log;
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _transientFaultHandler = transientFaultHandler;
        }

        public Task PublishAsync(CancellationToken cancellationToken, params RabbitMqMessage[] rabbitMqMessages)
        {
            return PublishAsync(rabbitMqMessages, cancellationToken);
        }

        public async Task PublishAsync(IReadOnlyCollection<RabbitMqMessage> rabbitMqMessages, CancellationToken cancellationToken)
        {
            var uri = _configuration.Uri;
            IRabbitConnection rabbitConnection = null;
            try
            {
                rabbitConnection = await GetRabbitMqConnectionAsync(uri, cancellationToken).ConfigureAwait(false);

                await _transientFaultHandler.TryAsync(
                    c => rabbitConnection.WithModelAsync(m => PublishAsync(m, rabbitMqMessages), c),
                    Label.Named("rabbitmq-publish"),
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (rabbitConnection != null)
                {
                    using (await _asyncLock.WaitAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        rabbitConnection.Dispose();
                        _connections.Remove(uri);
                    }
                }
                _log.LogError(e, "Failed to publish domain events to RabbitMQ");
                throw;
            }
        }

        private async Task<IRabbitConnection> GetRabbitMqConnectionAsync(Uri uri, CancellationToken cancellationToken)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                IRabbitConnection rabbitConnection;
                if (_connections.TryGetValue(uri, out rabbitConnection))
                {
                    return rabbitConnection;
                }

                rabbitConnection = await _connectionFactory.CreateConnectionAsync(uri, cancellationToken).ConfigureAwait(false);
                _connections.Add(uri, rabbitConnection);

                return rabbitConnection;
            }
        }

        private Task<int> PublishAsync(
            IModel model,
            IReadOnlyCollection<RabbitMqMessage> messages)
        {
            _log.LogInformation(
                "Publishing {0} domain events to RabbitMQ host '{1}'",
                messages.Count,
                _configuration.Uri.Host);

            foreach (var message in messages)
            {
                var bytes = Encoding.UTF8.GetBytes(message.Message);

                var basicProperties = model.CreateBasicProperties();
                basicProperties.Headers = message.Headers.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
                basicProperties.Persistent = _configuration.Persistent;
                basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.Now.ToUnixTime());
                basicProperties.ContentEncoding = "utf-8";
                basicProperties.ContentType = "application/json";
                basicProperties.MessageId = message.MessageId.Value;
                PublishSingleMessage(model, message, bytes, basicProperties);
            }

            return Task.FromResult(0);
        }

        protected virtual void PublishSingleMessage(IModel model, RabbitMqMessage message, byte[] bytes, IBasicProperties basicProperties)
        {
            model.BasicPublish(message.Exchange.Value, message.RoutingKey.Value, false, basicProperties, bytes);
        }

        public void Dispose()
        {
            foreach (var rabbitConnection in _connections.Values)
            {
                rabbitConnection.Dispose();
            }
            _connections.Clear();
        }
    }
}
