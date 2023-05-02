using softaware.Cqs.Decorators.ApplicationInsights;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to add application insights decorators.
/// </summary>
public static class SoftawareCqsApplicationInsightsDecoratorBuilderExtensions
{
    public static SoftawareCqsDecoratorBuilder AddApplicationInsightsDependencyTelemetryDecorator(this SoftawareCqsDecoratorBuilder decoratorBuilder)
    {
        decoratorBuilder.AddRequestHandlerDecorator(typeof(DependencyTelemetryRequestHandlerDecorator<,>));

        return decoratorBuilder;
    }
}
