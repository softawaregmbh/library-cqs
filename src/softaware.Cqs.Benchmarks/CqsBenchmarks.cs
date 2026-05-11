using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using softaware.Cqs.Benchmarks.Contracts.Commands;
using softaware.Cqs.Benchmarks.Contracts.Queries;

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
        this.runtimeServiceProvider = RuntimeSetup.CreateServiceProvider();
        this.runtimeProcessor = this.runtimeServiceProvider.GetRequiredService<IRequestProcessor>();

        // Setup compile-time (source-generated) approach
        this.generatedServiceProvider = SourceGeneratedSetup.CreateServiceProvider();
        this.generatedProcessor = this.generatedServiceProvider.GetRequiredService<IRequestProcessor>();
    }

    // --- Startup benchmarks ---

    [Benchmark(Description = "Runtime: DI Container Build")]
    public static IServiceProvider Runtime_ContainerBuild()
        => RuntimeSetup.CreateServiceProvider();

    [Benchmark(Description = "Generated: DI Container Build")]
    public static IServiceProvider Generated_ContainerBuild()
        => SourceGeneratedSetup.CreateServiceProvider();

    // --- Command execution benchmarks ---

    [Benchmark(Description = "Runtime: Execute SimpleCommand")]
    public async Task Runtime_SimpleCommand()
        => await this.runtimeProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);

    [Benchmark(Description = "Generated: Execute SimpleCommand")]
    public async Task Generated_SimpleCommand()
        => await this.generatedProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);

    // --- Query execution benchmarks ---

    [Benchmark(Description = "Runtime: Execute GetSquare")]
    public async Task<int> Runtime_GetSquare()
        => await this.runtimeProcessor.HandleAsync(new GetSquare { Value = 5 }, default);

    [Benchmark(Description = "Generated: Execute GetSquare")]
    public async Task<int> Generated_GetSquare()
        => await this.generatedProcessor.HandleAsync(new GetSquare { Value = 5 }, default);

    // --- Access-checked command (multiple decorator constraints) ---

    [Benchmark(Description = "Runtime: Execute AccessCheckedCommand")]
    public async Task Runtime_AccessCheckedCommand()
        => await this.runtimeProcessor.HandleAsync(new AccessCheckedCommand(), default);

    [Benchmark(Description = "Generated: Execute AccessCheckedCommand")]
    public async Task Generated_AccessCheckedCommand()
        => await this.generatedProcessor.HandleAsync(new AccessCheckedCommand(), default);

    // --- Query with caching + logging decorators ---

    [Benchmark(Description = "Runtime: Execute GetGreeting")]
    public async Task<string> Runtime_GetGreeting()
        => await this.runtimeProcessor.HandleAsync(new GetGreeting { Name = "World" }, default);

    [Benchmark(Description = "Generated: Execute GetGreeting")]
    public async Task<string> Generated_GetGreeting()
        => await this.generatedProcessor.HandleAsync(new GetGreeting { Name = "World" }, default);
}
