namespace KafkaFlow.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KafkaFlow.Producers;

    internal class ClusterConfigurationBuilder
        : IClusterConfigurationBuilder
    {
        private readonly IDependencyConfigurator dependencyConfigurator;

        private readonly List<ProducerConfigurationBuilder> producers = new List<ProducerConfigurationBuilder>();
        private readonly List<ConsumerConfigurationBuilder> consumers = new List<ConsumerConfigurationBuilder>();

        private IEnumerable<string> brokers;

        public ClusterConfigurationBuilder(IDependencyConfigurator dependencyConfigurator)
        {
            this.dependencyConfigurator = dependencyConfigurator;
        }

        public ClusterConfiguration Build(KafkaConfiguration kafkaConfiguration)
        {
            var configuration = new ClusterConfiguration(
                kafkaConfiguration,
                this.brokers.ToList());

            configuration.AddProducers(this.producers.Select(x => x.Build(configuration)));
            configuration.AddConsumers(this.consumers.Select(x => x.Build(configuration)));

            this.dependencyConfigurator.AddSingleton<IProducerAccessor>(
                resolver => new ProducerAccessor(
                    configuration.Producers.Select(producer => new MessageProducer(resolver, producer))));

            return configuration;
        }

        public IClusterConfigurationBuilder WithBrokers(IEnumerable<string> brokers)
        {
            this.brokers = brokers;
            return this;
        }

        public IClusterConfigurationBuilder AddProducer<TProducer>(Action<IProducerConfigurationBuilder> producer)
        {
            this.dependencyConfigurator.AddSingleton<IMessageProducer<TProducer>>(
                resolver => new MessageProducerWrapper<TProducer>(
                    resolver.Resolve<IProducerAccessor>().GetProducer<TProducer>()));

            return this.AddProducer(typeof(TProducer).FullName, producer);
        }

        public IClusterConfigurationBuilder AddProducer(string name, Action<IProducerConfigurationBuilder> producer)
        {
            var builder = new ProducerConfigurationBuilder(this.dependencyConfigurator, name);

            producer(builder);

            this.producers.Add(builder);

            return this;
        }

        public IClusterConfigurationBuilder AddConsumer(Action<IConsumerConfigurationBuilder> consumer)
        {
            var builder = new ConsumerConfigurationBuilder(this.dependencyConfigurator);

            consumer(builder);

            this.consumers.Add(builder);

            return this;
        }
    }
}
