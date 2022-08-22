using softaware.Cqs.Decorators.UsageAware;
using softaware.UsageAware;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add usage aware decorators.
/// </summary>
public static class SoftawareCqsUsageAwareDecoratorBuilderExtensions
{
    /// <summary>
    /// Registers the usage aware request loggers and decorators.
    /// </summary>
    /// <remarks>
    /// An instance of <see cref="IUsageAwareLogger"/> must be registered in the container.
    /// </remarks>
    /// <param name="decoratorBuilder">The CQS decorator builder.</param>
    /// <returns>The CQS decorator builder.</returns>
    public static SoftawareCqsDecoratorBuilder AddUsageAwareDecorators(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.Services.AddTransient(typeof(UsageAwareLogger<,>));

        decoratorBuilder.AddRequestHandlerDecorator(typeof(UsageAwareRequestHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
