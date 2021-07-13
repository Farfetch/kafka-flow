namespace KafkaFlow.Configuration
{
    using System;
    using System.Collections.Generic;
    using Confluent.Kafka;

    internal class ConsumerConfiguration : IConsumerConfiguration
    {
        private readonly ConsumerConfig consumerConfig;
        private int workersCount;

        public ConsumerConfiguration(
            ConsumerConfig consumerConfig,
            IEnumerable<string> topics,
            string consumerName,
            ClusterConfiguration clusterConfiguration,
            bool managementDisabled,
            int workersCount,
            int bufferSize,
            Factory<IDistributionStrategy> distributionStrategyFactory,
            IReadOnlyList<MiddlewareConfiguration> middlewaresConfigurations,
            bool autoStoreOffsets,
            TimeSpan autoCommitInterval,
            IReadOnlyList<Action<string>> statisticsHandlers,
            IReadOnlyList<Action<IDependencyResolver, List<TopicPartition>>> partitionsAssignedHandlers,
            IReadOnlyList<Action<IDependencyResolver, List<TopicPartitionOffset>>> partitionsRevokedHandlers,
            ConsumerCustomFactory customFactory)
        {
            this.consumerConfig = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));

            if (string.IsNullOrEmpty(this.consumerConfig.GroupId))
            {
                throw new ArgumentNullException(nameof(consumerConfig.GroupId));
            }

            this.DistributionStrategyFactory =
                distributionStrategyFactory ?? throw new ArgumentNullException(nameof(distributionStrategyFactory));
            this.MiddlewaresConfigurations = middlewaresConfigurations ?? throw new ArgumentNullException(nameof(middlewaresConfigurations));
            this.AutoStoreOffsets = autoStoreOffsets;
            this.AutoCommitInterval = autoCommitInterval;
            this.Topics = topics ?? throw new ArgumentNullException(nameof(topics));
            this.ConsumerName = consumerName ?? Guid.NewGuid().ToString();
            this.ClusterConfiguration = clusterConfiguration;
            this.ManagementDisabled = managementDisabled;
            this.WorkersCount = workersCount;
            this.StatisticsHandlers = statisticsHandlers;
            this.PartitionsAssignedHandlers = partitionsAssignedHandlers;
            this.PartitionsRevokedHandlers = partitionsRevokedHandlers;
            this.CustomFactory = customFactory;

            this.BufferSize = bufferSize > 0 ?
                bufferSize :
                throw new ArgumentOutOfRangeException(
                    nameof(bufferSize),
                    bufferSize,
                    "The value must be greater than 0");
        }

        public Factory<IDistributionStrategy> DistributionStrategyFactory { get; }

        public IReadOnlyList<MiddlewareConfiguration> MiddlewaresConfigurations { get; }

        public IEnumerable<string> Topics { get; }

        public string ConsumerName { get; }

        public ClusterConfiguration ClusterConfiguration { get; }

        public bool ManagementDisabled { get; }

        public int WorkersCount
        {
            get => this.workersCount;
            set =>
                this.workersCount = value > 0 ?
                    value :
                    throw new ArgumentOutOfRangeException(
                        nameof(this.WorkersCount),
                        this.WorkersCount,
                        $"The {nameof(this.WorkersCount)} value must be greater than 0");
        }

        public string GroupId => this.consumerConfig.GroupId;

        public int BufferSize { get; }

        public bool AutoStoreOffsets { get; }

        public TimeSpan AutoCommitInterval { get; }

        public IReadOnlyList<Action<string>> StatisticsHandlers { get; }

        public IReadOnlyList<Action<IDependencyResolver, List<TopicPartition>>> PartitionsAssignedHandlers { get; }

        public IReadOnlyList<Action<IDependencyResolver, List<TopicPartitionOffset>>> PartitionsRevokedHandlers { get; }

        public ConsumerCustomFactory CustomFactory { get; }

        public ConsumerConfig GetKafkaConfig()
        {
            return this.consumerConfig;
        }
    }
}
