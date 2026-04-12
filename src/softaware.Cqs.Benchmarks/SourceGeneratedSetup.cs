extern alias GeneratedDI;
using GeneratedDI::Microsoft.Extensions.DependencyInjection;
using softaware.Cqs.Benchmarks.Decorators;

namespace softaware.Cqs.Benchmarks;

/// <summary>
/// Sets up the source-generated DI container for benchmarking.
/// </summary>
internal static class SourceGeneratedSetup
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

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
