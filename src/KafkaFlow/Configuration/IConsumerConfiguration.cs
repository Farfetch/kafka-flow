namespace KafkaFlow.Configuration
{
    using System;
    using System.Collections.Generic;
    using Confluent.Kafka;

    /// <summary>
    /// Represents the Consumer configuration values
    /// </summary>
    public interface IConsumerConfiguration
    {
        /// <summary>
        /// Gets the consumer worker distribution strategy
        /// </summary>
        Factory<IDistributionStrategy> DistributionStrategyFactory { get; }

        /// <summary>
        /// Gets the consumer middleware configuration
        /// </summary>
        MiddlewareConfiguration MiddlewareConfiguration { get; }

        /// <summary>
        /// Gets the consumer configured topics
        /// </summary>
        IEnumerable<string> Topics { get; }

        /// <summary>
        /// Gets the consumer name
        /// </summary>
        string ConsumerName { get; }

        /// <summary>
        /// Gets or sets the current number of workers
        /// </summary>
        int WorkerCount { get; set; }

        /// <summary>
        /// Gets the consumer group
        /// </summary>
        string GroupId { get; }

        /// <summary>
        /// Gets the buffer size used for each worker
        /// </summary>
        int BufferSize { get; }

        /// <summary>
        /// Gets a value indicating whether if the application should store store at the end
        /// </summary>
        bool AutoStoreOffsets { get; }

        /// <summary>
        /// Gets the interval between commits
        /// </summary>
        TimeSpan AutoCommitInterval { get; }

        /// <summary>
        /// Gets the handlers used to collects statistics
        /// </summary>
        IReadOnlyList<Action<string>> StatisticsHandlers { get; }

        /// <summary>
        /// Gets the handlers that will be called when the partitions are assigned
        /// </summary>
        IReadOnlyList<Action<IDependencyResolver, List<TopicPartition>>> PartitionsAssignedHandlers { get; }

        /// <summary>
        /// Gets the handlers that will be called when the partitions are revoked
        /// </summary>
        IReadOnlyList<Action<IDependencyResolver, List<TopicPartitionOffset>>> PartitionsRevokedHandlers { get; }

        /// <summary>
        /// Gets the custom factory used to create a new <see cref="KafkaFlow.Consumers.IConsumer"/>
        /// </summary>
        ConsumerCustomFactory CustomFactory { get; }

        /// <summary>
        /// Parses KafkaFlow configuration to Confluent configuration
        /// </summary>
        /// <returns></returns>
        ConsumerConfig GetKafkaConfig();
    }
}
