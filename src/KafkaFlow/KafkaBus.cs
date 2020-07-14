namespace KafkaFlow
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using KafkaFlow.Configuration;
    using KafkaFlow.Consumers;
    using KafkaFlow.Producers;

    internal class KafkaBus : IKafkaBus
    {
        private readonly IDependencyResolver dependencyResolver;
        private readonly KafkaConfiguration configuration;
        private readonly IConsumerManager consumerManager;
        private readonly IProducerManager producerManager;
        private readonly ILogHandler logHandler;
        private readonly IList<KafkaConsumer> consumers = new List<KafkaConsumer>();

        public KafkaBus(
            IDependencyResolver dependencyResolver,
            IConsumerManager consumerManager,
            IProducerManager producerManager,
            ILogHandler logHandler,
            KafkaConfiguration configuration)
        {
            this.dependencyResolver = dependencyResolver;
            this.configuration = configuration;
            this.consumerManager = consumerManager;
            this.producerManager = producerManager;
            this.logHandler = logHandler;

            this.CreateProducers();
        }

        public IConsumerAccessor Consumers => this.consumerManager;

        public IProducerAccessor Producers => this.producerManager;

        private void CreateProducers()
        {
            foreach (var producerConfig in this.configuration.Clusters.SelectMany(x => x.Producers))
            {
                this.producerManager.AddOrUpdate(
                    new MessageProducer(
                        this.dependencyResolver,
                        producerConfig));
            }
        }

        public async Task StartAsync(CancellationToken stopCancellationToken = default)
        {
            foreach (var consumerConfiguration in this.configuration.Clusters.SelectMany(cl => cl.Consumers))
            {
                var dependencyScope = this.dependencyResolver.CreateScope();

                var middlewares = consumerConfiguration.MiddlewareConfiguration.Factories
                    .Select(factory => factory(dependencyScope.Resolver))
                    .ToList();

                var consumerWorkerPool = new ConsumerWorkerPool(
                    dependencyScope.Resolver,
                    consumerConfiguration,
                    this.logHandler,
                    new MiddlewareExecutor(middlewares),
                    consumerConfiguration.DistributionStrategyFactory);

                var consumer = new KafkaConsumer(
                    consumerConfiguration,
                    this.consumerManager,
                    this.logHandler,
                    consumerWorkerPool);

                this.consumers.Add(consumer);

                await consumer.StartAsync(stopCancellationToken).ConfigureAwait(false);
            }
        }

        public Task StopAsync()
        {
            return Task.WhenAll(this.consumers.Select(x => x.StopAsync()));
        }
    }
}
