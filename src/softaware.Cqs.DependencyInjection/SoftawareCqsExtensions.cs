#nullable enable

using System;
using softaware.Cqs;
using softaware.Cqs.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for the SimpleInjector container to configure the softaware CQS infrastructure.
    /// </summary>
    public static class SoftawareCqsExtensions
    {
        /// <summary>
        /// Adds the softaware CQS infrastructure and registers all required instances in the SimpleInjector container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="softawareCqsTypesBuilderAction">The types builder for registering assemblies from where to find <see cref="ICommandHandler{TCommand}"/> and <see cref="IQueryHandler{TQuery, TResult}"/> instances.</param>
        /// <returns>The softaware CQS builder.</returns>
        public static SoftawareCqsBuilder AddSoftawareCqs(
            this IServiceCollection services,
            Action<SoftawareCqsTypesBuilder> softawareCqsTypesBuilderAction)
        {
            var typesBuilder = new SoftawareCqsTypesBuilder();
            softawareCqsTypesBuilderAction.Invoke(typesBuilder);

            services
                .Scan(scan => scan
                    .FromAssemblies(typesBuilder.RegisteredAssemblies)
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());

            services.AddSingleton<IQueryProcessor, DynamicQueryProcessor>();
            services.AddSingleton<ICommandProcessor, DynamicCommandProcessor>();

            return new SoftawareCqsBuilder(services);
        }
    }
}
