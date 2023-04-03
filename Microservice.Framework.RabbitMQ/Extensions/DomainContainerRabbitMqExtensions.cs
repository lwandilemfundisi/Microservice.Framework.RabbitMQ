using Microservice.Framework.Domain;
using Microservice.Framework.Domain.Extensions;
using Microservice.Framework.Domain.Subscribers;
using Microservice.Framework.RabbitMQ.Integrations;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Framework.RabbitMQ.Extensions
{
    public static class DomainContainerRabbitMqExtensions
    {
        public static IDomainContainer PublishToRabbitMq(
            this IDomainContainer domainContainer,
            IRabbitMqConfiguration configuration)
        {
            return domainContainer.RegisterServices(sr =>
            {
                sr.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
                sr.AddSingleton<IRabbitMqMessageFactory, RabbitMqMessageFactory>();
                sr.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
                sr.AddSingleton<IRabbitMqResilientStrategy, RabbitMqResilientStrategy>();

                sr.AddSingleton(rc => configuration);

                sr.AddTransient<ISubscribeSynchronousToAll, RabbitMqDomainEventPublisher>();
            });
        }
    }
}
