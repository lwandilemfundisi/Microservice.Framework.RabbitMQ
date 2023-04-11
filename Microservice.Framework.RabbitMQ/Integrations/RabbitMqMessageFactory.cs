using Microservice.Framework.Domain.Events;
using Microservice.Framework.Domain.Events.Serializers;
using Microservice.Framework.RabbitMQ.Extensions;
using Microsoft.Extensions.Logging;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitMqMessageFactory : IRabbitMqMessageFactory
    {
        private readonly ILogger _log;
        private readonly IEventJsonSerializer _eventJsonSerializer;
        private readonly IRabbitMqConfiguration _rabbitMqConfiguration;

        public RabbitMqMessageFactory(
            ILogger log,
            IEventJsonSerializer eventJsonSerializer,
            IRabbitMqConfiguration rabbitMqConfiguration)
        {
            _log = log;
            _eventJsonSerializer = eventJsonSerializer;
            _rabbitMqConfiguration = rabbitMqConfiguration;
        }

        public RabbitMqMessage CreateMessage(IDomainEvent domainEvent)
        {
            var serializedEvent = _eventJsonSerializer.Serialize(
                domainEvent.GetAggregateEvent(),
                domainEvent.Metadata);

            var routingKey = new RoutingKey(string.Format(
                "domainevent.{0}.{1}.{2}",
                domainEvent.Metadata[MetadataKeys.AggregateName].ToSlug(),
                domainEvent.Metadata.EventName.ToSlug(),
                domainEvent.Metadata.EventVersion));
            var exchange = new Exchange(_rabbitMqConfiguration.Exchange);

            var rabbitMqMessage = new RabbitMqMessage(
                serializedEvent.SerializedData,
                domainEvent.Metadata,
                exchange,
                routingKey,
                new MessageId(domainEvent.Metadata[MetadataKeys.EventId]));

            _log.LogInformation("Create RabbitMQ message {0}", rabbitMqMessage);

            return rabbitMqMessage;
        }
    }
}
