namespace KafkaFlow
{
    /// <summary>
    /// Specifies the lifetime of a middleware
    /// </summary>
    public enum MiddlewareLifetime
    {
        /// <summary>
        /// Specifies that a single instance of the class will be created for the entire application
        /// </summary>
        Singleton,

        /// <summary>
        /// Specifies that a new instance of the class will be created for each scope
        /// </summary>
        Scoped,

        /// <summary>
        /// Specifies that a new instance of the class will be created every time it is requested
        /// </summary>
        Transient,

        /// <summary>
        /// Specifies that a new instance of the class will be created for each Consumer/Producer
        /// </summary>
        ConsumerOrProducer,

        /// <summary>
        /// Specifies that a new instance of the class will be created for each Consumer Worker
        /// For producers this setting will behave as <see cref="ConsumerOrProducer"/>
        /// </summary>
        Worker,
    }
}
