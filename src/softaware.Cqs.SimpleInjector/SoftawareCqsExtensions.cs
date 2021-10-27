#nullable enable

using System;
using SimpleInjector;
using softaware.Cqs.SimpleInjector;

namespace softaware.Cqs
{
    public static class SoftawareCqsExtensions
    {
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
