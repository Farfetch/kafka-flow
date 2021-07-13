namespace KafkaFlow.TypedHandler
{
    using System.Linq;
    using System.Threading.Tasks;

    internal class TypedHandlerMiddleware : IMessageMiddleware
    {
        private readonly IDependencyResolver dependencyResolver;
        private readonly TypedHandlerConfiguration configuration;

        public TypedHandlerMiddleware(
            IDependencyResolver dependencyResolver,
            TypedHandlerConfiguration configuration)
        {
            this.dependencyResolver = dependencyResolver;
            this.configuration = configuration;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            await Task.WhenAll(
                    this.configuration
                        .HandlerMapping
                        .GetHandlersTypes(context.Message.Value.GetType())
                        .Select(
                            handler =>
                                HandlerExecutor
                                    .GetExecutor(context.Message.Value.GetType())
                                    .Execute(
                                        this.dependencyResolver.Resolve(handler),
                                        context,
                                        context.Message.Value)))
                .ConfigureAwait(false);

            await next(context).ConfigureAwait(false);
        }
    }
}
