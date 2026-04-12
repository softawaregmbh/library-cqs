using BenchmarkDotNet.Running;
using softaware.Cqs;
using softaware.Cqs.Benchmarks;
using softaware.Cqs.Benchmarks.Contracts.Commands;

if (args.Length > 0 && args[0] == "--validate")
{
    Console.WriteLine("Validating DI setups...");

    // Validate runtime (Scrutor-based) approach
    var runtimeSp = RuntimeSetup.CreateServiceProvider();
    var runtimeProcessor = runtimeSp.GetRequiredService<IRequestProcessor>();
    await runtimeProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);
    Console.WriteLine("  Runtime (Scrutor): OK - IRequestProcessor resolved and handled command.");

    // Validate source-generated approach
    var generatedSp = SourceGeneratedSetup.CreateServiceProvider();
    var generatedProcessor = generatedSp.GetRequiredService<IRequestProcessor>();
    await generatedProcessor.HandleAsync(new SimpleCommand { Value = 1 }, default);
    Console.WriteLine("  Generated: OK - IRequestProcessor resolved and handled command.");

    Console.WriteLine("All validations passed!");
    return;
}

BenchmarkRunner.Run<CqsBenchmarks>(args: args);
