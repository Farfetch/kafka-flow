namespace KafkaFlow.Configuration
{
    using System;

    /// <summary>
    /// Represents a middleware configuration
    /// </summary>
    public class MiddlewareConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MiddlewareConfiguration"/> class.
        /// </summary>
        /// <param name="type">The middleware type</param>
        /// <param name="lifetime"><inheritdoc cref="Lifetime"/>The middleware instance lifetime</param>
        /// <param name="instanceContainerId">The instance container ID used to get the correct container when creating the instance</param>
        public MiddlewareConfiguration(Type type, MiddlewareLifetime lifetime, Guid? instanceContainerId = null)
        {
            this.Type = type;
            this.Lifetime = lifetime;
            this.InstanceContainerId = instanceContainerId;
        }

        /// <summary>
        /// Gets the middleware type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the middleware instance lifetime
        /// </summary>
        public MiddlewareLifetime Lifetime { get; }

        /// <summary>
        /// Gets the instance container ID used to get the desired factory when creating the instance
        /// </summary>
        public Guid? InstanceContainerId { get; }
    }
}
