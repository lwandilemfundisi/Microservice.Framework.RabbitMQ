using Microservice.Framework.Common;

namespace Microservice.Framework.RabbitMQ
{
    public class RabbitMqConfiguration 
        : IRabbitMqConfiguration
    {
        #region Constructors

        private RabbitMqConfiguration(Uri uri, bool persistent, int modelsPrConnection, string exchange)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (string.IsNullOrEmpty(exchange)) throw new ArgumentNullException(nameof(exchange));

            Uri = uri;
            Persistent = persistent;
            ModelsPrConnection = modelsPrConnection;
            Exchange = exchange;
        }

        #endregion

        #region IRabbitMqConfiguration Members

        public Uri Uri { get; }
        public bool Persistent { get; }
        public int ModelsPrConnection { get; }
        public string Exchange { get; }

        #endregion

        #region Methods

        public static IRabbitMqConfiguration With(
            Uri uri,
            bool persistent = true,
            int modelsPrConnection = 5,
            string exchange = "")
        {
            if(exchange.IsNullOrEmpty()) throw new ArgumentNullException(nameof(exchange));

            return new RabbitMqConfiguration(uri, persistent, modelsPrConnection, exchange);
        }

        #endregion
    }
}
