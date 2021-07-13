namespace KafkaFlow.UnitTests.ConfigurationBuilders
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Confluent.Kafka;
    using FluentAssertions;
    using KafkaFlow.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using AutoOffsetReset = KafkaFlow.AutoOffsetReset;

    [TestClass]
    public class ConsumerConfigurationBuilderTests
    {
        private readonly Fixture fixture = new();

        private Mock<IDependencyConfigurator> dependencyConfiguratorMock;

        private ConsumerConfigurationBuilder target;

        [TestInitialize]
        public void Setup()
        {
            this.dependencyConfiguratorMock = new Mock<IDependencyConfigurator>();

            this.target = new ConsumerConfigurationBuilder(this.dependencyConfiguratorMock.Object);
        }

        [TestMethod]
        public void DependencyConfigurator_SetProperty_ReturnPassedInstance()
        {
            // Assert
            this.target.DependencyConfigurator.Should().Be(this.dependencyConfiguratorMock.Object);
        }

        [TestMethod]
        public void Build_RequiredCalls_ReturnDefaultValues()
        {
            // Arrange
            var clusterConfiguration = this.fixture.Create<ClusterConfiguration>();
            var topic1 = this.fixture.Create<string>();
            const int bufferSize = 100;
            const int workers = 10;
            var groupId = this.fixture.Create<string>();

            this.target
                .Topics(topic1)
                .WithBufferSize(bufferSize)
                .WithWorkersCount(workers)
                .WithGroupId(groupId);

            // Act
            var configuration = this.target.Build(clusterConfiguration);

            // Assert
            configuration.Topics.Should().BeEquivalentTo(topic1);
            configuration.BufferSize.Should().Be(bufferSize);
            configuration.WorkersCount.Should().Be(workers);
            configuration.GroupId.Should().Be(groupId);
            configuration.GetKafkaConfig().AutoOffsetReset.Should().BeNull();
            configuration.GetKafkaConfig().EnableAutoOffsetStore.Should().Be(false);
            configuration.GetKafkaConfig().EnableAutoCommit.Should().Be(false);
            configuration.AutoStoreOffsets.Should().Be(true);
            configuration.AutoCommitInterval.Should().Be(TimeSpan.FromSeconds(5));
            configuration.StatisticsHandlers.Should().BeEmpty();
            configuration.PartitionsAssignedHandlers.Should().BeEmpty();
            configuration.PartitionsRevokedHandlers.Should().BeEmpty();
            configuration.MiddlewaresConfigurations.Should().BeEmpty();
        }

        [TestMethod]
        public void Build_AllCalls_ReturnPassedValues()
        {
            // Arrange
            var clusterConfiguration = this.fixture.Create<ClusterConfiguration>();
            var topic1 = this.fixture.Create<string>();
            var topic2 = this.fixture.Create<string>();
            var name = this.fixture.Create<string>();
            const int bufferSize = 100;
            const int workers = 10;
            const AutoOffsetReset offsetReset = AutoOffsetReset.Earliest;
            var groupId = this.fixture.Create<string>();
            const int autoCommitInterval = 10000;
            const int maxPollIntervalMs = 500000;
            ConsumerCustomFactory customFactory = (producer, _) => producer;
            Action<string> statisticsHandler = _ => { };
            Action<IDependencyResolver, List<TopicPartition>> partitionsAssignedHandler = (_, _) => { };
            Action<IDependencyResolver, List<TopicPartitionOffset>> partitionsRevokedHandler = (_, _) => { };
            const int statisticsIntervalMs = 100;
            var consumerConfig = new ConsumerConfig();

            this.target
                .Topics(topic1)
                .Topic(topic2)
                .WithName(name)
                .WithBufferSize(bufferSize)
                .WithWorkersCount(workers)
                .WithGroupId(groupId)
                .WithAutoOffsetReset(offsetReset)
                .WithManualStoreOffsets()
                .WithAutoCommitIntervalMs(autoCommitInterval)
                .WithMaxPollIntervalMs(maxPollIntervalMs)
                .WithConsumerConfig(consumerConfig)
                .WithCustomFactory(customFactory)
                .WithStatisticsIntervalMs(statisticsIntervalMs)
                .WithStatisticsHandler(statisticsHandler)
                .WithPartitionsAssignedHandler(partitionsAssignedHandler)
                .WithPartitionsRevokedHandler(partitionsRevokedHandler)
                .AddMiddlewares(m => m.Add<IMessageMiddleware>());

            // Act
            var configuration = this.target.Build(clusterConfiguration);

            // Assert
            configuration.Topics.Should().BeEquivalentTo(topic1, topic2);
            configuration.ConsumerName.Should().Be(name);
            configuration.BufferSize.Should().Be(bufferSize);
            configuration.WorkersCount.Should().Be(workers);
            configuration.GroupId.Should().Be(groupId);
            configuration.GetKafkaConfig().AutoOffsetReset.Should().Be(offsetReset);
            configuration.AutoStoreOffsets.Should().Be(false);
            configuration.GetKafkaConfig().EnableAutoOffsetStore.Should().Be(false);
            configuration.GetKafkaConfig().EnableAutoCommit.Should().Be(false);
            configuration.AutoCommitInterval.Should().Be(TimeSpan.FromMilliseconds(autoCommitInterval));
            configuration.GetKafkaConfig().MaxPollIntervalMs.Should().Be(maxPollIntervalMs);
            configuration.GetKafkaConfig().StatisticsIntervalMs.Should().Be(statisticsIntervalMs);
            configuration.StatisticsHandlers.Should().HaveElementAt(0, statisticsHandler);
            configuration.PartitionsAssignedHandlers.Should().HaveElementAt(0, partitionsAssignedHandler);
            configuration.PartitionsRevokedHandlers.Should().HaveElementAt(0, partitionsRevokedHandler);
            configuration.GetKafkaConfig().Should().BeSameAs(consumerConfig);
            configuration.MiddlewaresConfigurations.Should().HaveCount(1);
        }
    }
}
