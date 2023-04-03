namespace Microservice.Framework.RabbitMQ.Integrations
{
    public class RabbitMqMessage
    {
        public MessageId MessageId { get; }
        public string Message { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Exchange Exchange { get; }
        public RoutingKey RoutingKey { get; }

        public RabbitMqMessage(
            string message,
            IReadOnlyDictionary<string, string> headers,
            Exchange exchange,
            RoutingKey routingKey,
            MessageId messageId)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (exchange == null) throw new ArgumentNullException(nameof(exchange));
            if (routingKey == null) throw new ArgumentNullException(nameof(routingKey));
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));

            Message = message;
            Headers = headers;
            Exchange = exchange;
            RoutingKey = routingKey;
            MessageId = messageId;
        }

        public override string ToString()
        {
            return $"{{Exchange: {Exchange}, RoutingKey: {RoutingKey}, MessageId: {MessageId}, Headers: {Headers.Count}, Bytes: {Message.Length / 2}}}";
        }
    }
}
