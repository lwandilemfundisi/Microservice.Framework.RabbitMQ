using Microservice.Framework.Common;

namespace Microservice.Framework.RabbitMQ
{
    public class Exchange : SingleValueObject<string>
    {
        public static Exchange Default => new Exchange(string.Empty);

        public Exchange(string value) : base(value)
        {
        }
    }
}
