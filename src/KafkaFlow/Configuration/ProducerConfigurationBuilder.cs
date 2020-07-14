namespace KafkaFlow.Configuration
{
    using System;
    using Confluent.Kafka;
    using Acks = KafkaFlow.Acks;

    internal class ProducerConfigurationBuilder : IProducerConfigurationBuilder
    {
        private readonly string name;
        private readonly ProducerMiddlewareConfigurationBuilder middlewareConfigurationBuilder;

        private string topic;
        private ProducerConfig baseProducerConfig;
        private Acks? acks;

        public ProducerConfigurationBuilder(IDependencyConfigurator dependencyConfigurator, string name)
        {
            this.name = name;
            this.DependencyConfigurator = dependencyConfigurator;
            this.middlewareConfigurationBuilder = new ProducerMiddlewareConfigurationBuilder(dependencyConfigurator);
        }

        public IDependencyConfigurator DependencyConfigurator { get; }

        public IProducerConfigurationBuilder AddMiddlewares(Action<IProducerMiddlewareConfigurationBuilder> middlewares)
        {
            middlewares(this.middlewareConfigurationBuilder);
            return this;
        }

        public IProducerConfigurationBuilder DefaultTopic(string topic)
        {
            this.topic = topic;
            return this;
        }

        public IProducerConfigurationBuilder WithProducerConfig(ProducerConfig config)
        {
            this.baseProducerConfig = config;
            return this;
        }

        public IProducerConfigurationBuilder WithAcks(Acks acks)
        {
            this.acks = acks;
            return this;
        }

        public ProducerConfiguration Build(ClusterConfiguration clusterConfiguration)
        {
            var configuration = new ProducerConfiguration(
                clusterConfiguration,
                this.name,
                this.topic,
                this.acks,
                this.middlewareConfigurationBuilder.Build(),
                this.baseProducerConfig ?? new ProducerConfig());

            return configuration;
        }
    }
}
