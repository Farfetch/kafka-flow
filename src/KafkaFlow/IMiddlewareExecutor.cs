namespace KafkaFlow
{
    using System;
    using System.Threading.Tasks;

    internal interface IMiddlewareExecutor
    {
        Task Execute(
            IDependencyResolver dependencyResolver,
            IMessageContext context,
            Func<IMessageContext, Task> nextOperation);
    }
}
