namespace KafkaFlow
{
    using System.Linq;
    using KafkaFlow.Configuration;
    using KafkaFlow.Consumers;

    internal class ConsumerManagerFactory : IConsumerManagerFactory
    {
        public IConsumerManager Create(IConsumerConfiguration configuration, IDependencyResolver resolver)
        {
            var logHandler = resolver.Resolve<ILogHandler>();

            var middlewares = configuration.MiddlewareConfiguration.Factories
                .Select(factory => factory(resolver))
                .ToList();

            var consumer = configuration.CustomFactory(new Consumer(configuration, resolver, logHandler), resolver);

            var consumerWorkerPool = new ConsumerWorkerPool(
                consumer,
                resolver,
                logHandler,
                new MiddlewareExecutor(middlewares),
                configuration.DistributionStrategyFactory);

            var feeder = new WorkerPoolFeeder(
                consumer,
                consumerWorkerPool,
                logHandler);

            var consumerManager = new ConsumerManager(
                consumer,
                consumerWorkerPool,
                feeder,
                logHandler);

            return consumerManager;
        }
    }
}
