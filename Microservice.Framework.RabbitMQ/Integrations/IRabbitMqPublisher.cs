namespace Microservice.Framework.RabbitMQ.Integrations
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(
            CancellationToken cancellationToken, 
            params RabbitMqMessage[] rabbitMqMessages);

        Task PublishAsync(
            IReadOnlyCollection<RabbitMqMessage> rabbitMqMessages, 
            CancellationToken cancellationToken);
    }
}
