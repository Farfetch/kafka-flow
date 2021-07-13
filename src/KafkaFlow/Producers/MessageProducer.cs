namespace KafkaFlow.Producers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using KafkaFlow.Configuration;

    internal class MessageProducer : IMessageProducer, IDisposable
    {
        private readonly IDependencyResolver dependencyResolver;
        private readonly IProducerConfiguration configuration;
        private readonly MiddlewareExecutor middlewareExecutor;

        private readonly object producerCreationSync = new();

        private volatile IProducer<byte[], byte[]> producer;

        public MessageProducer(
            IDependencyResolver dependencyResolver,
            IProducerConfiguration configuration)
        {
            this.dependencyResolver = dependencyResolver;
            this.configuration = configuration;
            this.middlewareExecutor = new MiddlewareExecutor(configuration.MiddlewaresConfigurations);
        }

        public string ProducerName => this.configuration.Name;

        public async Task<DeliveryResult<byte[], byte[]>> ProduceAsync(
            string topic,
            object messageKey,
            object messageValue,
            IMessageHeaders headers = null)
        {
            DeliveryResult<byte[], byte[]> report = null;

            using var scope = this.dependencyResolver.CreateScope();

            await this.middlewareExecutor
                .Execute(
                    scope.Resolver,
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
            var scope = this.dependencyResolver.CreateScope();

            this.middlewareExecutor
                .Execute(
                    scope.Resolver,
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
                    })
                .ContinueWith(_ => scope.Dispose());
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

        public void Dispose()
        {
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

                var producerBuilder = new ProducerBuilder<byte[], byte[]>(this.configuration.GetKafkaConfig())
                    .SetErrorHandler(
                        (_, error) =>
                        {
                            if (error.IsFatal)
                            {
                                this.InvalidateProducer(error, null);
                            }
                            else
                            {
                                this.dependencyResolver
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

                return this.producer = this.configuration.CustomFactory(
                    producerBuilder.Build(),
                    this.dependencyResolver);
            }
        }

        private void InvalidateProducer(Error error, DeliveryResult<byte[], byte[]> result)
        {
            lock (this.producerCreationSync)
            {
                this.producer = null;
            }

            this.dependencyResolver
                .Resolve<ILogHandler>()
                .Error(
                    "Kafka produce fatal error occurred. The producer will be recreated",
                    result is null ? new KafkaException(error) : new ProduceException<byte[], byte[]>(error, result),
                    new { Error = error });
        }

        private async Task<DeliveryResult<byte[], byte[]>> InternalProduceAsync(IMessageContext context)
        {
            DeliveryResult<byte[], byte[]> result = null;

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

            return result;
        }

        private void InternalProduce(
            IMessageContext context,
            Action<DeliveryReport<byte[], byte[]>> deliveryHandler)
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
