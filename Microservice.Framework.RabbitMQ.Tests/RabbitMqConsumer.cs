using Microservice.Framework.Common;
using Microservice.Framework.RabbitMQ.Integrations;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Microservice.Framework.RabbitMQ.Tests
{
    public class RabbitMqConsumer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly EventingBasicConsumer _eventingBasicConsumer;
        private readonly BlockingCollection<BasicDeliverEventArgs> _receivedMessages = new BlockingCollection<BasicDeliverEventArgs>();

        public RabbitMqConsumer(
            Uri uri, 
            Exchange exchange, 
            IEnumerable<string> routingKeys)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = uri,
            };
            _connection = connectionFactory.CreateConnection();
            _model = _connection.CreateModel();

            _model.ExchangeDeclare(exchange.Value, ExchangeType.Topic, false);

            var queueName = $"test-{Guid.NewGuid():N}";
            _model.QueueDeclare(
                queueName,
                false,
                false,
                true,
                null);

            foreach (var routingKey in routingKeys)
            {
                _model.QueueBind(
                    queueName,
                    exchange.Value,
                    routingKey,
                    null);
            }

            _eventingBasicConsumer = new EventingBasicConsumer(_model);
            _eventingBasicConsumer.Received += OnReceived;

            _model.BasicConsume(queueName, false, _eventingBasicConsumer);
        }

        private void OnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            _receivedMessages.Add(basicDeliverEventArgs);
        }

        public IReadOnlyCollection<RabbitMqMessage> GetMessages(TimeSpan timeout, int count = 1)
        {
            var rabbitMqMessages = new List<RabbitMqMessage>();
            var stopwatch = Stopwatch.StartNew();

            while (rabbitMqMessages.Count < count)
            {
                if (stopwatch.Elapsed >= timeout)
                {
                    throw new TimeoutException($"Timed out after {stopwatch.Elapsed.TotalSeconds:0.##} seconds");
                }

                if (!_receivedMessages.TryTake(out var basicDeliverEventArgs, TimeSpan.FromMilliseconds(100)))
                {
                    continue;
                }

                rabbitMqMessages.Add(CreateRabbitMqMessage(basicDeliverEventArgs));
            }

            return rabbitMqMessages;
        }

        private static RabbitMqMessage CreateRabbitMqMessage(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var headers = basicDeliverEventArgs.BasicProperties.Headers
            .ToDictionary(kv => kv.Key, kv => Encoding.UTF8.GetString((byte[])kv.Value));
            var message = Encoding.UTF8.GetString(basicDeliverEventArgs.Body.ToArray());

            return new RabbitMqMessage(
                message,
                headers,
            new Exchange(basicDeliverEventArgs.Exchange),
                new RoutingKey(basicDeliverEventArgs.RoutingKey),
                new MessageId(basicDeliverEventArgs.BasicProperties.MessageId));
        }

        public void Dispose()
        {
            _eventingBasicConsumer.Received -= OnReceived;
            _model.Dispose();
            _connection.Dispose();
            _receivedMessages.Dispose();
        }
    }
}
