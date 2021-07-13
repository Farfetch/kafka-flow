namespace KafkaFlow.Configuration
{
    using System;
    using System.Collections.Generic;

    internal class MiddlewareConfigurationBuilder<TBuilder>
        : IMiddlewareConfigurationBuilder<TBuilder>
        where TBuilder : class, IMiddlewareConfigurationBuilder<TBuilder>
    {
        private readonly List<MiddlewareConfiguration> middlewaresConfigurations = new();

        protected MiddlewareConfigurationBuilder(IDependencyConfigurator dependencyConfigurator)
        {
            this.DependencyConfigurator = dependencyConfigurator;
        }

        public IDependencyConfigurator DependencyConfigurator { get; }

        public TBuilder Add<T>(
            Factory<T> factory,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            return this.AddAt(this.middlewaresConfigurations.Count, factory, lifetime);
        }

        public TBuilder AddAtBeginning<T>(
            Factory<T> factory,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            return this.AddAt(0, factory, lifetime);
        }

        public TBuilder Add<T>(MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            return this.AddAt<T>(this.middlewaresConfigurations.Count, lifetime);
        }

        public TBuilder AddAtBeginning<T>(MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            return this.AddAt<T>(0, lifetime);
        }

        public IReadOnlyList<MiddlewareConfiguration> Build() => this.middlewaresConfigurations;

        private static InstanceLifetime ParseLifetime(MiddlewareLifetime lifetime)
        {
            return lifetime switch
            {
                MiddlewareLifetime.Scoped => InstanceLifetime.Scoped,
                MiddlewareLifetime.Singleton => InstanceLifetime.Singleton,
                MiddlewareLifetime.Transient or
                    MiddlewareLifetime.Worker or
                    MiddlewareLifetime.ConsumerOrProducer => InstanceLifetime.Transient,
                _ => throw new InvalidCastException()
            };
        }

        private TBuilder AddAt<T>(
            int position,
            Factory<T> factory,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            var containerId = Guid.NewGuid();

            this.DependencyConfigurator.Add(
                typeof(MiddlewareInstanceContainer<T>),
                _ => new MiddlewareInstanceContainer<T>(containerId, factory),
                ParseLifetime(lifetime));

            this.middlewaresConfigurations.Insert(
                position,
                new MiddlewareConfiguration(typeof(T), lifetime, containerId));

            return this as TBuilder;
        }

        private TBuilder AddAt<T>(
            int position,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware
        {
            this.DependencyConfigurator.Add<T>(ParseLifetime(lifetime));

            this.middlewaresConfigurations.Insert(
                position,
                new MiddlewareConfiguration(typeof(T), lifetime));

            return this as TBuilder;
        }
    }
}
