using Microsoft.Extensions.Logging;

namespace Microservice.Framework.RabbitMQ.Extensions
{
    public static class DisposableExtensions
    {
        public static void DisposeSafe(
            this IDisposable disposable,
            ILogger logger,
            string message)
        {
            if (disposable == null) return;

            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, message);
            }
        }
    }
}
