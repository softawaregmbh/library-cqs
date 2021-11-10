#nullable enable

using System;
using SimpleInjector;
using softaware.Cqs.SimpleInjector;

namespace softaware.Cqs
{
    /// <summary>
    /// Provides extension methods for the SimpleInjector container to configure the softaware CQS infrastructure.
    /// </summary>
    public static class SoftawareCqsExtensions
    {
        /// <summary>
        /// Adds the softaware CQS infrastructure and registers all required instances in the SimpleInjector container.
        /// </summary>
        /// <param name="container">The SimpleInjector container.</param>
        /// <param name="softawareCqsTypesBuilderAction">The types builder for registering assemblies from where to find <see cref="ICommandHandler{TCommand}"/> and <see cref="IQueryHandler{TQuery, TResult}"/> instances.</param>
        /// <returns>The softaware CQS builder.</returns>
        public static SoftawareCqsBuilder AddSoftawareCqs(
            this Container container,
            Action<SoftawareCqsTypesBuilder> softawareCqsTypesBuilderAction)
        {
            container.RegisterInstance<IQueryProcessor>(new DynamicQueryProcessor(container));
            container.RegisterInstance<ICommandProcessor>(new DynamicCommandProcessor(container));

            var typesBuilder = new SoftawareCqsTypesBuilder();
            softawareCqsTypesBuilderAction.Invoke(typesBuilder);

            container.Register(typeof(ICommandHandler<>), typesBuilder.RegisteredAssemblies);
            container.Register(typeof(IQueryHandler<,>), typesBuilder.RegisteredAssemblies);

            return new SoftawareCqsBuilder(container);
        }
    }
}
