using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using softaware.Cqs;
using softaware.Cqs.Benchmarks.Contracts.Commands;
using softaware.Cqs.Benchmarks.Contracts.Queries;
using softaware.Cqs.Benchmarks.Decorators;

namespace softaware.Cqs.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CqsBenchmarks
{
    private IRequestProcessor runtimeProcessor = null!;
    private IRequestProcessor generatedProcessor = null!;
    private IServiceProvider runtimeServiceProvider = null!;
    private IServiceProvider generatedServiceProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup runtime (Scrutor-based) approach
        runtimeServiceProvider = RuntimeSetup.CreateServiceProvider();
        runtimeProcessor = runtimeServiceProvider.GetRequiredService<IRequestProcessor>();

        // Setup compile-time (source-generated) approach
        generatedServiceProvider = SourceGeneratedSetup.CreateServiceProvider();
        generatedProcessor = generatedServiceProvider.GetRequiredService<IRequestProcessor>();
    }

    // --- Startup benchmarks ---

    [Benchmark(Description = "Runtime: DI Container Build")]
    public IServiceProvider Runtime_ContainerBuild()
        => RuntimeSetup.CreateServiceProvider();

    [Benchmark(Description = "Generated: DI Container Build")]
    public IServiceProvider Generated_ContainerBuild()
        => SourceGeneratedSetup.CreateServiceProvider();

    // --- Command execution benchmarks ---

    [Benchmark(Description = "Runtime: Execute SimpleCommand")]
    public async Task Runtime_SimpleCommand()
        => await runtimeProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);

    [Benchmark(Description = "Generated: Execute SimpleCommand")]
    public async Task Generated_SimpleCommand()
        => await generatedProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);

    // --- Query execution benchmarks ---

    [Benchmark(Description = "Runtime: Execute GetSquare")]
    public async Task<int> Runtime_GetSquare()
        => await runtimeProcessor.HandleAsync(new GetSquare { Value = 5 }, default);

    [Benchmark(Description = "Generated: Execute GetSquare")]
    public async Task<int> Generated_GetSquare()
        => await generatedProcessor.HandleAsync(new GetSquare { Value = 5 }, default);

    // --- Access-checked command (multiple decorator constraints) ---

    [Benchmark(Description = "Runtime: Execute AccessCheckedCommand")]
    public async Task Runtime_AccessCheckedCommand()
        => await runtimeProcessor.HandleAsync(new AccessCheckedCommand(), default);

    [Benchmark(Description = "Generated: Execute AccessCheckedCommand")]
    public async Task Generated_AccessCheckedCommand()
        => await generatedProcessor.HandleAsync(new AccessCheckedCommand(), default);

    // --- Query with caching + logging decorators ---

    [Benchmark(Description = "Runtime: Execute GetGreeting")]
    public async Task<string> Runtime_GetGreeting()
        => await runtimeProcessor.HandleAsync(new GetGreeting { Name = "World" }, default);

    [Benchmark(Description = "Generated: Execute GetGreeting")]
    public async Task<string> Generated_GetGreeting()
        => await generatedProcessor.HandleAsync(new GetGreeting { Name = "World" }, default);
}
