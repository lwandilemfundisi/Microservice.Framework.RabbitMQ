using Microservice.Framework.Domain.Events;
using Microservice.Framework.Domain.Subscribers;
using Microservice.Framework.RabbitMQ.Integrations;

namespace Microservice.Framework.RabbitMQ
{
    public class RabbitMqDomainEventPublisher : ISubscribeSynchronousToAll
    {
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IRabbitMqMessageFactory _rabbitMqMessageFactory;

        public RabbitMqDomainEventPublisher(
            IRabbitMqPublisher rabbitMqPublisher,
            IRabbitMqMessageFactory rabbitMqMessageFactory)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
            _rabbitMqMessageFactory = rabbitMqMessageFactory;
        }

        public Task HandleAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            var rabbitMqMessages = domainEvents.Select(e => _rabbitMqMessageFactory.CreateMessage(e)).ToList();

            return _rabbitMqPublisher.PublishAsync(rabbitMqMessages, cancellationToken);
        }
    }
}
