namespace KafkaFlow.UnitTests.Consumer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Confluent.Kafka;
    using FluentAssertions;
    using KafkaFlow.Configuration;
    using KafkaFlow.Consumers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ConsumerManagerTests
    {
        private readonly Fixture fixture = new();

        private ConsumerManager target;

        private Mock<IConsumer> consumerMock;
        private Mock<IConsumerWorkerPool> workerPoolMock;
        private Mock<IWorkerPoolFeeder> feederMock;
        private Mock<ILogHandler> logHandlerMock;
        private Mock<IDependencyResolver> dependencyResolver;

        private Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartition>> onPartitionAssignedHandler;
        private Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>> onPartitionRevokedHandler;

        [TestInitialize]
        public void Setup()
        {
            this.consumerMock = new Mock<IConsumer>(MockBehavior.Strict);
            this.workerPoolMock = new Mock<IConsumerWorkerPool>(MockBehavior.Strict);
            this.feederMock = new Mock<IWorkerPoolFeeder>(MockBehavior.Strict);
            this.logHandlerMock = new Mock<ILogHandler>(MockBehavior.Strict);
            this.dependencyResolver = new Mock<IDependencyResolver>();

            this.consumerMock
                .Setup(x => x.OnPartitionsAssigned(It.IsAny<Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartition>>>()))
                .Callback((Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartition>> value) => this.onPartitionAssignedHandler = value);

            this.consumerMock
                .Setup(x => x.OnPartitionsRevoked(It.IsAny<Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>>>()))
                .Callback((Action<IDependencyResolver, IConsumer<byte[], byte[]>, List<TopicPartitionOffset>> value) => this.onPartitionRevokedHandler = value);

            this.target = new ConsumerManager(
                this.consumerMock.Object,
                this.workerPoolMock.Object,
                this.feederMock.Object,
                this.logHandlerMock.Object);
        }

        [TestMethod]
        public void ConstructorCalled_InitializeProperties()
        {
            // Assert
            this.target.Consumer.Should().Be(this.consumerMock.Object);
            this.target.WorkerPool.Should().Be(this.workerPoolMock.Object);
            this.target.Feeder.Should().Be(this.feederMock.Object);

            this.consumerMock.VerifyAll();
        }

        [TestMethod]
        public async Task StartAsync_StartDependencies()
        {
            // Arrange
            this.feederMock
                .Setup(x => x.Start());

            // Act
            await this.target.StartAsync();

            // Assert
            this.feederMock.VerifyAll();
        }

        [TestMethod]
        public async Task StopAsync_StopDependencies()
        {
            // Arrange
            this.feederMock
                .Setup(x => x.StopAsync())
                .Returns(Task.CompletedTask);

            this.workerPoolMock
                .Setup(x => x.StopAsync())
                .Returns(Task.CompletedTask);

            this.consumerMock
                .Setup(x => x.Dispose());

            // Act
            await this.target.StopAsync();

            // Assert
            this.feederMock.VerifyAll();
            this.workerPoolMock.VerifyAll();
            this.consumerMock.VerifyAll();
        }

        [TestMethod]
        public void OnPartitionsAssigned_StartWorkerPool()
        {
            // Arrange
            IConsumer<byte[], byte[]> consumer = null;
            var partitions = this.fixture.Create<List<TopicPartition>>();

            this.workerPoolMock
                .Setup(x => x.StartAsync(partitions))
                .Returns(Task.CompletedTask);

            this.consumerMock
                .SetupGet(x => x.Configuration)
                .Returns(new Mock<IConsumerConfiguration>().Object);

            this.logHandlerMock
                .Setup(x => x.Info(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            this.onPartitionAssignedHandler(this.dependencyResolver.Object, consumer, partitions);

            // Assert
            this.workerPoolMock.VerifyAll();
            this.logHandlerMock.VerifyAll();
        }

        [TestMethod]
        public void OnPartitionsRevoked_StopWorkerPool()
        {
            // Arrange
            IConsumer<byte[], byte[]> consumer = null;
            var partitions = this.fixture.Create<List<TopicPartitionOffset>>();

            this.workerPoolMock
                .Setup(x => x.StopAsync())
                .Returns(Task.CompletedTask);

            this.consumerMock
                .SetupGet(x => x.Configuration)
                .Returns(new Mock<IConsumerConfiguration>().Object);

            this.logHandlerMock
                .Setup(x => x.Warning(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            this.onPartitionRevokedHandler(this.dependencyResolver.Object, consumer, partitions);

            // Assert
            this.workerPoolMock.VerifyAll();
            this.logHandlerMock.VerifyAll();
        }
    }
}
