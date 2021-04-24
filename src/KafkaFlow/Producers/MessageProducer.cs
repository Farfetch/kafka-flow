namespace KafkaFlow.Producers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using KafkaFlow.Configuration;

    internal class MessageProducer : IMessageProducer, IConsumerProducerTransactionCoordinator, IDisposable
    {
        private readonly IProducerConfiguration configuration;
        private readonly MiddlewareExecutor middlewareExecutor;
        private readonly IDependencyResolverScope dependencyResolverScope;

        private readonly object producerCreationSync = new();
        private readonly ManualResetEvent waitHandle = new(true);

        private volatile IProducer<byte[], byte[]> producer;

        public MessageProducer(
            IDependencyResolver dependencyResolver,
            IProducerConfiguration configuration)
        {
            this.configuration = configuration;

            // Create middlewares instances inside a scope to allow scoped injections in producer middlewares
            this.dependencyResolverScope = dependencyResolver.CreateScope();

            var middlewares = this.configuration.MiddlewareConfiguration.Factories
                .Select(factory => factory(this.dependencyResolverScope.Resolver))
                .ToList();

            this.middlewareExecutor = new MiddlewareExecutor(middlewares);
        }

        public string ProducerName => this.configuration.Name;

        public string ProducerId => this.producer.Name;

        public async Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
            string topic,
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null)
        {
            DeliveryResult<byte[], byte[]> report = null;

            await this.middlewareExecutor
                .Execute(
                    new MessageContext(
                        new Message(messageKey, messageValue),
                        headers,
                        null,
                        new ProducerContext(topic)),
                    async context =>
                    {
                        report = await this
                            .InternalProduceAsync(context)
                            .ConfigureAwait(false);
                    })
                .ConfigureAwait(false);

            return report;
        }

        public Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null)
        {
            if (string.IsNullOrWhiteSpace(this.configuration.DefaultTopic))
            {
                throw new InvalidOperationException(
                    $"There is no default topic defined for producer {this.ProducerName}");
            }

            return this.ProduceAsync(
                this.configuration.DefaultTopic,
                messageKey,
                messageValue,
                headers);
        }

        public void Produce(
            string topic,
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
        {
            this.middlewareExecutor.Execute(
                new MessageContext(
                    new Message(messageKey, messageValue),
                    headers,
                    null,
                    new ProducerContext(topic)),
                context =>
                {
                    var completionSource = new TaskCompletionSource<byte>();

                    this.InternalProduce(
                        context,
                        report =>
                        {
                            if (report.Error.IsError)
                            {
                                completionSource.SetException(new ProduceException<byte[], byte[]>(report.Error, report));
                            }
                            else
                            {
                                completionSource.SetResult(0);
                            }

                            deliveryHandler?.Invoke(report);
                        });

                    return completionSource.Task;
                });
        }

        public void Produce(
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler = null)
        {
            if (string.IsNullOrWhiteSpace(this.configuration.DefaultTopic))
            {
                throw new InvalidOperationException(
                    $"There is no default topic defined for producer {this.ProducerName}");
            }

            this.Produce(
                this.configuration.DefaultTopic,
                messageKey,
                messageValue,
                headers,
                deliveryHandler);
        }

        public void RegisterConsumerProducerTransaction(IConsumerContext consumerContext)
        {
            if (string.IsNullOrWhiteSpace(this.configuration.GetKafkaConfig().TransactionalId))
            {
                throw new InvalidOperationException("Producer not configured to support transaction");
            }

            // StoreOffset is disabled as consumer offsets are committed inside the producer transaction
            consumerContext.ShouldStoreOffset = false;
            consumerContext.RegisterProducer(this.EnsureProducer(), this);
        }

        /// <inheritdoc />
        void IConsumerProducerTransactionCoordinator.Initiated()
        {
            this.waitHandle.Reset();
        }

        /// <inheritdoc />
        void IConsumerProducerTransactionCoordinator.Completed()
        {
            this.waitHandle.Set();
        }

        public void Dispose()
        {
            this.dependencyResolverScope.Dispose();
            this.producer?.Dispose();
        }

        private static void FillContextWithResultMetadata(IMessageContext context, DeliveryResult<byte[], byte[]> result)
        {
            var concreteProducerContext = (ProducerContext) context.ProducerContext;

            concreteProducerContext.Offset = result.Offset;
            concreteProducerContext.Partition = result.Partition;
        }

        private static Message<byte[], byte[]> CreateMessage(IMessageContext context)
        {
            if (context.Message.Value is not byte[] value)
            {
                throw new InvalidOperationException(
                    $"The message value must be a byte array to be produced, it is a {context.Message.Value.GetType().FullName}." +
                    "You should serialize or encode your message object using a middleware");
            }

            var key = context.Message.Key switch
            {
                string stringKey => Encoding.UTF8.GetBytes(stringKey),
                byte[] bytesKey => bytesKey,
                _ => throw new InvalidOperationException(
                    $"The message key must be a byte array or a string to be produced, it is a {context.Message.Key.GetType().FullName}." +
                    "You should serialize or encode your message object using a middleware")
            };

            return new()
            {
                Key = key,
                Value = value,
                Headers = ((MessageHeaders) context.Headers).GetKafkaHeaders(),
                Timestamp = Timestamp.Default,
            };
        }

        private IProducer<byte[], byte[]> EnsureProducer()
        {
            if (this.producer != null)
            {
                return this.producer;
            }

            lock (this.producerCreationSync)
            {
                if (this.producer != null)
                {
                    return this.producer;
                }

                var producerConfig = this.configuration.GetKafkaConfig();

                var producerBuilder = new ProducerBuilder<byte[], byte[]>(producerConfig)
                    .SetErrorHandler(
                        (_, error) =>
                        {
                            if (error.IsFatal)
                            {
                                this.InvalidateProducer(error, null);
                            }
                            else
                            {
                                this.dependencyResolverScope.Resolver
                                    .Resolve<ILogHandler>()
                                    .Warning("Kafka Producer Error", new { Error = error });
                            }
                        })
                    .SetStatisticsHandler(
                        (_, statistics) =>
                        {
                            foreach (var handler in this.configuration.StatisticsHandlers)
                            {
                                handler.Invoke(statistics);
                            }
                        });

                this.producer = this.configuration.CustomFactory(
                    producerBuilder.Build(),
                    this.dependencyResolverScope.Resolver);

                if (!string.IsNullOrWhiteSpace(producerConfig.TransactionalId))
                {
                    this.producer.InitTransactions(TimeSpan.FromMilliseconds(producerConfig.TransactionTimeoutMs.Value));
                    this.producer.BeginTransaction();
                }

                return this.producer;
            }
        }

        private void InvalidateProducer(Error error, DeliveryResult<byte[], byte[]> result)
        {
            lock (this.producerCreationSync)
            {
                this.producer = null;
            }

            this.dependencyResolverScope.Resolver
                .Resolve<ILogHandler>()
                .Error(
                    "Kafka produce fatal error occurred. The producer will be recreated",
                    result is null ? new KafkaException(error) : new ProduceException<byte[], byte[]>(error, result),
                    new { Error = error });
        }

        private async Task<DeliveryResult<byte[], byte[]>> InternalProduceAsync(IMessageContext context)
        {
            DeliveryResult<byte[], byte[]> result = null;

            if (this.waitHandle.WaitOne())
            {
                try
                {
                    result = await this
                        .EnsureProducer()
                        .ProduceAsync(
                            context.ProducerContext.Topic,
                            CreateMessage(context))
                        .ConfigureAwait(false);
                }
                catch (ProduceException<byte[], byte[]> e)
                {
                    if (e.Error.IsFatal)
                    {
                        this.InvalidateProducer(e.Error, result);
                    }

                    throw;
                }

                FillContextWithResultMetadata(context, result);
            }

            return result;
        }

        private void InternalProduce(
            IMessageContext context,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler)
        {
            if (this.waitHandle.WaitOne())
            {
                this
                    .EnsureProducer()
                    .Produce(
                        context.ProducerContext.Topic,
                        CreateMessage(context),
                        report =>
                        {
                            if (report.Error.IsFatal)
                            {
                                this.InvalidateProducer(report.Error, report);
                            }

                            FillContextWithResultMetadata(context, report);

                            deliveryHandler(report);
                        });
            }
        }
    }
}
