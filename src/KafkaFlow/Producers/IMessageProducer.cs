namespace KafkaFlow
{
    using System;
    using System.Threading.Tasks;
    using Confluent.Kafka;

    /// <summary>
    /// Provides access to the kafka message producer
    /// </summary>
    /// <typeparam name="TProducer">The producer associated type</typeparam>
    public interface IMessageProducer<TProducer> : IMessageProducer
    {
    }

    /// <summary>
    /// Provides access to the kafka producer
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// Gets the unique producer's name defined in the configuration
        /// </summary>
        string ProducerName { get; }

        /// <summary>
        /// Gets producerId
        /// </summary>
        string ProducerId { get; }

        /// <summary>
        /// Produces a new message
        /// </summary>
        /// <param name="topic">The topic where the message wil be produced</param>
        /// <param name="messageKey">The message key</param>
        /// <param name="messageValue">The message value</param>
        /// <param name="headers">The message headers</param>
        /// <returns></returns>
        Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
            string topic,
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null);

        /// <summary>
        /// Produces a new message in the configured default topic
        /// </summary>
        /// <param name="messageKey">The message key</param>
        /// <param name="messageValue">The message value</param>
        /// <param name="headers">The message headers</param>
        /// <returns></returns>
        Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null);

        /// <summary>
        /// Produces a new message
        /// This should be used for high throughput scenarios: <see href="https://github.com/confluentinc/confluent-kafka-dotnet/wiki/Producer#produceasync-vs-produce"/>
        /// </summary>
        /// <param name="topic">The topic where the message wil be produced</param>
        /// <param name="messageKey">The message key</param>
        /// <param name="messageValue">The message value</param>
        /// <param name="headers">The message headers</param>
        /// <param name="deliveryHandler">A handler with the operation result</param>
        void Produce(
            string topic,
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null);

        /// <summary>
        /// Produces a new message in the configured default topic
        /// This should be used for high throughput scenarios: <see href="https://github.com/confluentinc/confluent-kafka-dotnet/wiki/Producer#produceasync-vs-produce"/>
        /// </summary>
        /// <param name="messageKey">The message key</param>
        /// <param name="messageValue">The message value</param>
        /// <param name="headers">The message headers</param>
        /// <param name="deliveryHandler">A handler with the operation result</param>
        void Produce(
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null);

        /// <summary>
        /// Register consumer/producer as part of a transaction
        /// </summary>
        /// <param name="consumerContext">Consumer context</param>
        void RegisterConsumerProducerTransaction(IConsumerContext consumerContext);
    }
}
