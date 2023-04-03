using FluentAssertions;
using Microservice.Framework.Domain;
using Microservice.Framework.Domain.Events;
using Microservice.Framework.Domain.Extensions;
using Microservice.Framework.RabbitMQ.Extensions;
using Microservice.Framework.RabbitMQ.Integrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Microservice.Framework.RabbitMQ.Tests
{
    public class PerfomanceFixture
    {
        private Uri _uri;
        private CancellationTokenSource _timeout;

        [SetUp]
        public void Setup()
        {
            _uri = new Uri("amqp://localhost");
            _timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        }

        [TearDown]
        public void TearDown()
        {
            _timeout.Dispose();
        }

        [Test]
        public async Task PerfomanceTest()
        {
            var exchange = new Exchange($"test-exchange-{Guid.NewGuid():N}");
            var routingKey = new RoutingKey("perf");
            var exceptions = new ConcurrentBag<Exception>();
            const int taskCount = 10;
            const int messagesPrThread = 200;
            const int totalMessageCount = taskCount * messagesPrThread;

            using (var consumer = new RabbitMqConsumer(_uri, exchange, new[] { "#" }))
            using (var provider = BuildDomainContainer(exchange, s => s.RegisterServices(r => r.AddLogging(l => l.AddConsole()))))
            {
                var rabbitMqPublisher = provider.GetService<IRabbitMqPublisher>();

                var tasks = Enumerable.Range(0, taskCount)
                    .Select(i => Task.Run(() => SendMessagesAsync(
                        rabbitMqPublisher, 
                        messagesPrThread, 
                        exchange, 
                        routingKey, 
                        exceptions, 
                        _timeout.Token)));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                var rabbitMqMessages = consumer.GetMessages(TimeSpan.FromMinutes(1), totalMessageCount);
                rabbitMqMessages.Should().HaveCount(totalMessageCount);
                exceptions.Should().BeEmpty();
            }
        }

        #region Private Methods

        private static async Task SendMessagesAsync(
            IRabbitMqPublisher rabbitMqPublisher,
            int count,
            Exchange exchange,
            RoutingKey routingKey,
            ConcurrentBag<Exception> exceptions,
            CancellationToken cancellationToken)
        {
            var guid = Guid.NewGuid();

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var rabbitMqMessage = new RabbitMqMessage(
                        $"{guid}-{i}",
                        new Metadata(KeyValuePair.Create("test", "test")),
                        exchange,
                        routingKey,
                        new MessageId(Guid.NewGuid().ToString("D")))
                    {

                    };
                    await rabbitMqPublisher.PublishAsync(cancellationToken, rabbitMqMessage).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        private ServiceProvider BuildDomainContainer(
            Exchange exchange,
            Func<IDomainContainer, IDomainContainer> configure = null)
        {
            configure = configure ?? (e => e);

            return configure(DomainContainer.New()
                .PublishToRabbitMq(RabbitMqConfiguration.With(_uri, false, exchange: exchange.Value))
                .AddDefaults(GetType().Assembly))
                .ServiceCollection
                .BuildServiceProvider();
                
        }

        #endregion
    }
}