using Microservice.Framework.Common;
using RabbitMQ.Client.Exceptions;

namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitMqResilientStrategy : IRabbitMqResilientStrategy
    {
        private static readonly ISet<Type> TransientExceptions = new HashSet<Type>
            {
                typeof(EndOfStreamException),
                typeof(BrokerUnreachableException),
                typeof(OperationInterruptedException)
            };

        public Repeat CheckRetry(Exception exception, TimeSpan totalExecutionTime, int currentRetryCount)
        {
            return currentRetryCount <= 3 && TransientExceptions.Contains(exception.GetType())
                ? Repeat.YesAfter(TimeSpan.FromMilliseconds(25))
                : Repeat.No;
        }
    }
}
