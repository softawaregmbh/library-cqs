extern alias RuntimeDI;
using RuntimeDI::Microsoft.Extensions.DependencyInjection;
using softaware.Cqs.Benchmarks.Decorators;

namespace softaware.Cqs.Benchmarks;

/// <summary>
/// Sets up the runtime (Scrutor-based) DI container for benchmarking.
/// </summary>
internal static class RuntimeSetup
{
    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

#pragma warning disable CQ0008 // This file uses the runtime (Scrutor-based) DI package, not the source generator
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(RuntimeSetup).Assembly))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(LoggingDecorator<,>))
                .AddRequestHandlerDecorator(typeof(TransactionDecorator<,>))
                .AddRequestHandlerDecorator(typeof(CachingDecorator<,>))
                .AddRequestHandlerDecorator(typeof(AccessCheckDecorator<,>))
                .AddRequestHandlerDecorator(typeof(PriorityDecorator<,>)));
#pragma warning restore CQ0008

        return services.BuildServiceProvider();
    }
}
