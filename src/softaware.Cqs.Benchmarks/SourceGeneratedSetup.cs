extern alias GeneratedDI;

using softaware.Cqs;
using softaware.Cqs.Benchmarks.Decorators;
using GeneratedDI::Microsoft.Extensions.DependencyInjection;

namespace softaware.Cqs.Benchmarks;

/// <summary>
/// Sets up the source-generated DI container for benchmarking.
/// </summary>
internal static class SourceGeneratedSetup
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new global::Microsoft.Extensions.DependencyInjection.ServiceCollection();

        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(SourceGeneratedSetup)))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(LoggingDecorator<,>))
                .AddRequestHandlerDecorator(typeof(TransactionDecorator<,>))
                .AddRequestHandlerDecorator(typeof(CachingDecorator<,>))
                .AddRequestHandlerDecorator(typeof(AccessCheckDecorator<,>))
                .AddRequestHandlerDecorator(typeof(PriorityDecorator<,>)));

        return services.BuildServiceProvider();
    }
}
