using Microservice.Framework.Common;

namespace Microservice.Framework.RabbitMQ
{
    public class MessageId : SingleValueObject<string>, IIdentity
    {
        public MessageId(string value) : base(value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
        }
    }
}
