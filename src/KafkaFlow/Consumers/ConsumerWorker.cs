namespace KafkaFlow.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Confluent.Kafka;

    internal class ConsumerWorker : IConsumerWorker
    {
        private readonly IConsumer consumer;
        private readonly IDependencyResolver dependencyResolver;
        private readonly IOffsetManager offsetManager;
        private readonly IMiddlewareExecutor middlewareExecutor;
        private readonly ILogHandler logHandler;

        private readonly Channel<ConsumeResult<byte[], byte[]>> messagesBuffer;

        private CancellationTokenSource stopCancellationTokenSource;
        private Task backgroundTask;
        private Action onMessageFinishedHandler;

        public ConsumerWorker(
            IConsumer consumer,
            IDependencyResolver dependencyResolver,
            int workerId,
            IOffsetManager offsetManager,
            IMiddlewareExecutor middlewareExecutor,
            ILogHandler logHandler)
        {
            this.Id = workerId;
            this.consumer = consumer;
            this.dependencyResolver = dependencyResolver;
            this.offsetManager = offsetManager;
            this.middlewareExecutor = middlewareExecutor;
            this.logHandler = logHandler;
            this.messagesBuffer = Channel.CreateBounded<ConsumeResult<byte[], byte[]>>(consumer.Configuration.BufferSize);
        }

        public int Id { get; }

        public ValueTask EnqueueAsync(
            ConsumeResult<byte[], byte[]> message,
            CancellationToken stopCancellationToken = default)
        {
            return this.messagesBuffer.Writer.WriteAsync(message, stopCancellationToken);
        }

        public Task StartAsync()
        {
            this.stopCancellationTokenSource = new CancellationTokenSource();

            this.backgroundTask = Task.Run(
                async () =>
                {
                    while (!this.stopCancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            var message = await this.messagesBuffer.Reader
                                .ReadAsync(this.stopCancellationTokenSource.Token)
                                .ConfigureAwait(false);

                            var context = new MessageContext(
                                new Message(message.Message.Key, message.Message.Value),
                                new MessageHeaders(message.Message.Headers),
                                new ConsumerContext(
                                    this.consumer,
                                    this.offsetManager,
                                    message,
                                    this.stopCancellationTokenSource.Token,
                                    this.Id),
                                null);

                            try
                            {
                                using var scope = this.dependencyResolver.CreateScope();

                                await this.middlewareExecutor
                                    .Execute(scope.Resolver, context, _ => Task.CompletedTask)
                                    .ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                this.logHandler.Error(
                                    "Error executing consumer",
                                    ex,
                                    new
                                    {
                                        context.Message,
                                        context.ConsumerContext.Topic,
                                        MessageKey = context.Message.Key,
                                        context.ConsumerContext.ConsumerName,
                                    });
                            }
                            finally
                            {
                                if (this.consumer.Configuration.AutoStoreOffsets && context.ConsumerContext.ShouldStoreOffset)
                                {
                                    this.offsetManager.StoreOffset(message.TopicPartitionOffset);
                                }

                                this.onMessageFinishedHandler?.Invoke();
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Ignores the exception
                        }
                    }
                });

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (this.stopCancellationTokenSource.Token.CanBeCanceled)
            {
                this.stopCancellationTokenSource.Cancel();
            }

            await this.backgroundTask.ConfigureAwait(false);
            this.backgroundTask.Dispose();
        }

        public void OnTaskCompleted(Action handler)
        {
            this.onMessageFinishedHandler = handler;
        }
    }
}
