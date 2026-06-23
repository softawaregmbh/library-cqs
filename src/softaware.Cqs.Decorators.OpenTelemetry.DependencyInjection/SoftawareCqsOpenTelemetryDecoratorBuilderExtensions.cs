using softaware.Cqs.Decorators.OpenTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add OpenTelemetry decorators.
/// </summary>
public static class SoftawareCqsOpenTelemetryDecoratorBuilderExtensions
{
    public static SoftawareCqsDecoratorBuilder AddOpenTelemetryActivityDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.AddRequestHandlerDecorator(typeof(ActivityRequestHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
