namespace KafkaFlow.UnitTests.ConfigurationBuilders
{
    using System;
    using AutoFixture;
    using Confluent.Kafka;
    using FluentAssertions;
    using KafkaFlow.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProducerConfigurationBuilderTests
    {
        private readonly Fixture fixture = new Fixture();

        private Mock<IDependencyConfigurator> dependencyConfiguratorMock;

        private string name;

        private ProducerConfigurationBuilder target;

        [TestInitialize]
        public void Setup()
        {
            this.dependencyConfiguratorMock = new Mock<IDependencyConfigurator>();
            this.name = this.fixture.Create<string>();

            this.target = new ProducerConfigurationBuilder(
                this.dependencyConfiguratorMock.Object,
                this.name);
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

            // Act
            var configuration = this.target.Build(clusterConfiguration);

            // Assert
            configuration.Cluster.Should().Be(clusterConfiguration);
            configuration.Name.Should().Be(this.name);
            configuration.DefaultTopic.Should().BeNull();
            configuration.Acks.Should().BeNull();
            configuration.StatisticsHandlers.Should().BeEmpty();
            configuration.MiddlewareConfiguration.Factories.Should().BeEmpty();
        }

        [TestMethod]
        public void Build_AllCalls_ReturnPassedValues()
        {
            // Arrange
            var clusterConfiguration = this.fixture.Create<ClusterConfiguration>();

            var defaultTopic = this.fixture.Create<string>();
            var acks = this.fixture.Create<KafkaFlow.Acks>();
            const int lingerMs = 50;
            ProducerCustomFactory customFactory = (producer, resolver) => producer;
            Action<string> statisticsHandler = s => { };
            const int statisticsIntervalMs = 100;
            var producerConfig = new ProducerConfig();

            this.target
                .DefaultTopic(defaultTopic)
                .WithAcks(acks)
                .WithLingerMs(lingerMs)
                .WithCustomFactory(customFactory)
                .WithStatisticsHandler(statisticsHandler)
                .WithStatisticsIntervalMs(statisticsIntervalMs)
                .WithProducerConfig(producerConfig)
                .AddMiddlewares(m => m.Add<IMessageMiddleware>());

            // Act
            var configuration = this.target.Build(clusterConfiguration);

            // Assert
            configuration.Cluster.Should().Be(clusterConfiguration);
            configuration.Name.Should().Be(this.name);
            configuration.DefaultTopic.Should().Be(defaultTopic);
            configuration.Acks.Should().Be(acks);
            configuration.BaseProducerConfig.LingerMs.Should().Be(lingerMs);
            configuration.BaseProducerConfig.StatisticsIntervalMs.Should().Be(statisticsIntervalMs);
            configuration.StatisticsHandlers.Should().HaveElementAt(0, statisticsHandler);
            configuration.BaseProducerConfig.Should().BeSameAs(producerConfig);
            configuration.MiddlewareConfiguration.Factories.Should().HaveCount(1);
        }
    }
}
