namespace Microservice.Framework.RabbitMQ
{
    public interface IRabbitMqConfiguration
    {
        Uri Uri { get; }
        bool Persistent { get; }
        int ModelsPrConnection { get; }
        string Exchange { get; }
    }
}
