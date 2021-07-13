namespace KafkaFlow.Configuration
{
    using System;

    internal interface IMiddlewareInstanceContainer
    {
        Guid Id { get; }

        IMessageMiddleware GetInstance(IDependencyResolver resolver);
    }
}
