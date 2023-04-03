using Microservice.Framework.Common;

namespace Microservice.Framework.RabbitMQ
{
    public class RoutingKey : SingleValueObject<string>
    {
        public RoutingKey(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
        }
    }
}
