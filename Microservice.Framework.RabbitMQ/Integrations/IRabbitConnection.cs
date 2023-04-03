using RabbitMQ.Client;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public interface IRabbitConnection : IDisposable
    {
        Task<int> WithModelAsync(
            Func<IModel, Task> action, 
            CancellationToken cancellationToken);
    }
}
