namespace KafkaFlow.Unity
{
    using System;
    using System.Collections.Generic;
    using global::Unity;

    /// <summary>
    /// Unity implementation of <see cref="IDependencyResolver"/>
    /// </summary>
    public class UnityDependencyResolver : IDependencyResolver
    {
        private readonly IUnityContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">A Unity container instance</param>
        public UnityDependencyResolver(IUnityContainer container) => this.container = container;

        /// <inheritdoc cref="IDependencyResolver.CreateScope"/>
        public IDependencyResolverScope CreateScope() =>
            new UnityDependencyResolverScope(this.container.CreateChildContainer());

        /// <inheritdoc cref="IDependencyResolver.Resolve"/>
        public object Resolve(Type type) => this.container.Resolve(type);

        /// <inheritdoc cref="IDependencyResolver.ResolveAll"/>
        public IEnumerable<object> ResolveAll(Type type) => this.container.ResolveAll(type);
    }
}
