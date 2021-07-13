namespace KafkaFlow.Configuration
{
    using System;

    internal class MiddlewareInstanceContainer<T> : IMiddlewareInstanceContainer
    {
        private readonly object sync = new();
        private readonly Factory<IMessageMiddleware> factory;

        private IMessageMiddleware instance;

        public MiddlewareInstanceContainer(Guid id, Factory<IMessageMiddleware> factory)
        {
            this.Id = id;
            this.factory = factory;
        }

        public Guid Id { get; }

        public IMessageMiddleware GetInstance(IDependencyResolver resolver)
        {
            if (this.instance is not null)
            {
                return this.instance;
            }

            lock (this.sync)
            {
                if (this.instance is not null)
                {
                    return this.instance;
                }

                this.instance = this.factory(resolver);
            }

            return this.instance;
        }
    }
}
