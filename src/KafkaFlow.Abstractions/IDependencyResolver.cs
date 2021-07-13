namespace KafkaFlow
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the interface of a dependency injection resolver
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Creates the scope lifetime of the dependency injection resolver
        /// </summary>
        /// <returns>The <see cref="IDependencyResolverScope"/> created</returns>
        IDependencyResolverScope CreateScope();

        /// <summary>
        /// Resolve an instance of the requested type
        /// </summary>
        /// <param name="type"><see cref="Type"/> of object to be resolved</param>
        /// <returns>The retrieved object</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolve all instances configured for the given type
        /// </summary>
        /// <param name="type"><see cref="Type"/> of object to be resolved</param>
        /// <returns></returns>
        IEnumerable<object> ResolveAll(Type type);
    }
}
