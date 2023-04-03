using Microservice.Framework.Domain.Events;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public interface IRabbitMqMessageFactory
    {
        RabbitMqMessage CreateMessage(IDomainEvent domainEvent);
    }
}
