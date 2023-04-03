using Microservice.Framework.Common;
using Microservice.Framework.RabbitMQ.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitConnection : IRabbitConnection
    {
        private readonly ILogger _log;
        private readonly IConnection _connection;
        private readonly AsyncLock _asyncLock;
        private readonly ConcurrentBag<IModel> _models;

        public RabbitConnection(ILogger log, int maxModels, IConnection connection)
        {
            _connection = connection;
            _log = log;
            _asyncLock = new AsyncLock(maxModels);
            _models = new ConcurrentBag<IModel>(
                Enumerable
                .Range(0, maxModels)
                .Select(_ => connection.CreateModel()));
        }

        public async Task<int> WithModelAsync(Func<IModel, Task> action, CancellationToken cancellationToken)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                IModel model;
                if (!_models.TryTake(out model))
                {
                    throw new InvalidOperationException(
                        "This should NEVER happen! If it does, please report a bug.");
                }

                try
                {
                    await action(model).ConfigureAwait(false);
                }
                finally
                {
                    _models.Add(model);
                }
            }

            return 0;
        }

        public void Dispose()
        {
            foreach (var model in _models)
            {
                model.DisposeSafe(_log, "Failed to dispose model");
            }
            _connection.DisposeSafe(_log, "Failed to dispose connection");
            _log.LogInformation("Disposing RabbitMQ connection");
        }
    }
}
