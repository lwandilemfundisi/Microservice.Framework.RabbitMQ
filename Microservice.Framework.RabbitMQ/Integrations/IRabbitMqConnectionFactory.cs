namespace Microservice.Framework.RabbitMQ.Integrations
{
    public interface IRabbitMqConnectionFactory
    {
        Task<IRabbitConnection> CreateConnectionAsync(
            Uri uri, 
            CancellationToken cancellationToken);
    }
}
