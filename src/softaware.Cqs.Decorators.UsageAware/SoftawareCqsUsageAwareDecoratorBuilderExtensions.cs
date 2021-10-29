using softaware.Cqs.Decorators.UsageAware;
using softaware.UsageAware;

namespace softaware.Cqs
{
    /// <summary>
    /// Provides extension methods to add usage aware decorators.
    /// </summary>
    public static class SoftawareCqsUsageAwareDecoratorBuilderExtensions
    {
        /// <summary>
        /// Registers the usage aware command and query loggers and decorators.
        /// </summary>
        /// <remarks>
        /// An instance of <see cref="IUsageAwareLogger"/> must be registered in the container.
        /// </remarks>
        /// <param name="decoratorBuilder">The CQS decorator builder.</param>
        /// <returns>The CQS decorator builder.</returns>
        public static SoftawareCqsDecoratorBuilder AddUsageAwareDecorators(this SoftawareCqsDecoratorBuilder decoratorBuilder)
        {
            decoratorBuilder.Container.Register(typeof(UsageAwareCommandLogger<>));
            decoratorBuilder.Container.Register(typeof(UsageAwareQueryLogger<,>));

            decoratorBuilder.Container.RegisterDecorator(typeof(ICommandHandler<>), typeof(UsageAwareCommandHandlerDecorator<>));
            decoratorBuilder.Container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(UsageAwareQueryHandlerDecorator<,>));

            return decoratorBuilder;
        }
    }
}
