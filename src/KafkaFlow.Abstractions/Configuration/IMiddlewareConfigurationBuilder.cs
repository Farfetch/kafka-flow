namespace KafkaFlow.Configuration
{
    /// <summary>
    /// Used to build the middlewares configuration
    /// </summary>
    /// <typeparam name="TBuilder">The middleware builder interface type</typeparam>
    public interface IMiddlewareConfigurationBuilder<out TBuilder>
        where TBuilder : IMiddlewareConfigurationBuilder<TBuilder>
    {
        /// <summary>
        /// Gets the dependency injection configurator
        /// </summary>
        IDependencyConfigurator DependencyConfigurator { get; }

        /// <summary>
        /// Registers a middleware
        /// </summary>
        /// <param name="factory">A factory to create the instance</param>
        /// <param name="lifetime">The middleware instance lifetime</param>
        /// <typeparam name="T">A class that implements the <see cref="IMessageMiddleware"/></typeparam>
        /// <returns></returns>
        TBuilder Add<T>(
            Factory<T> factory,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware;

        /// <summary>
        /// Registers a middleware at the beginning of the middleware list
        /// The middleware will run before other middlewares that already have been registered
        /// </summary>
        /// <param name="factory">A factory to create the instance</param>
        /// <param name="lifetime">The middleware instance lifetime</param>
        /// <typeparam name="T">A class that implements the <see cref="IMessageMiddleware"/></typeparam>
        /// <returns></returns>
        TBuilder AddAtBeginning<T>(
            Factory<T> factory,
            MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware;

        /// <summary>
        /// Registers a middleware
        /// </summary>
        /// <param name="lifetime">The middleware instance lifetime</param>
        /// <typeparam name="T">A class that implements the <see cref="IMessageMiddleware"/></typeparam>
        /// <returns></returns>
        TBuilder Add<T>(MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware;

        /// <summary>
        /// Registers a middleware at the beginning of the middleware list
        /// The middleware will run before other middlewares that already have been registered
        /// </summary>
        /// <param name="lifetime">The middleware instance lifetime</param>
        /// <typeparam name="T">A class that implements the <see cref="IMessageMiddleware"/></typeparam>
        /// <returns></returns>
        TBuilder AddAtBeginning<T>(MiddlewareLifetime lifetime = MiddlewareLifetime.ConsumerOrProducer)
            where T : class, IMessageMiddleware;
    }
}
