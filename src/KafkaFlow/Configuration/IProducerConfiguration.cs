namespace KafkaFlow.Configuration
{
    using System;
    using System.Collections.Generic;
    using Confluent.Kafka;
    using Acks = KafkaFlow.Acks;

    /// <summary>
    /// Represents the producer configuration values
    /// </summary>
    public interface IProducerConfiguration
    {
        /// <summary>
        /// Gets the cluster configuration
        /// </summary>
        ClusterConfiguration Cluster { get; }

        /// <summary>
        /// Gets the cluster name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the default topic to be used when publishing
        /// </summary>
        string DefaultTopic { get; }

        /// <summary>
        /// Gets the confluent producer configuration
        /// </summary>
        ProducerConfig BaseProducerConfig { get; }

        /// <summary>
        /// Gets the acknowledge type
        /// </summary>
        Acks? Acks { get; }

        /// <summary>
        /// Gets the middlewares configurations
        /// </summary>
        IReadOnlyList<MiddlewareConfiguration> MiddlewaresConfigurations { get; }

        /// <summary>
        /// Gets the statistics handlers
        /// </summary>
        IReadOnlyList<Action<string>> StatisticsHandlers { get; }

        /// <summary>
        /// Gets the producer custom factory
        /// </summary>
        ProducerCustomFactory CustomFactory { get; }

        /// <summary>
        /// Gets the producer configuration
        /// </summary>
        /// <returns></returns>
        ProducerConfig GetKafkaConfig();
    }
}
