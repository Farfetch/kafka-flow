namespace KafkaFlow.Unity
{
    using global::Unity;

    internal class UnityDependencyResolverScope : IDependencyResolverScope
    {
        private readonly IUnityContainer container;

        public UnityDependencyResolverScope(IUnityContainer container)
        {
            this.container = container;
            this.Resolver = new UnityDependencyResolver(container);
        }

        public IDependencyResolver Resolver { get; }

        public void Dispose() => this.container.Dispose();
    }
}
