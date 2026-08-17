using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace softaware.Cqs.DependencyInjection.SourceGenerator.Tests;

internal static class TestHelper
{
    /// <summary>
    /// Runs the CQS source generator against the given source code and returns the result.
    /// </summary>
    public static GeneratorDriverRunResult RunGenerator(string source, params string[] additionalSources)
    {
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(source)
        };

        foreach (var additional in additionalSources)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(additional));
        }

        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: references,
            options: CreateCompilationOptions());

        var generator = new CqsSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return driver.GetRunResult();
    }

    /// <summary>
    /// Runs the generator and returns the output compilation and diagnostics.
    /// </summary>
    public static (Compilation OutputCompilation, ImmutableArray<Diagnostic> Diagnostics, GeneratorDriverRunResult RunResult)
        RunGeneratorWithCompilation(string source, params string[] additionalSources)
    {
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(source)
        };

        foreach (var additional in additionalSources)
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(additional));
        }

        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: references,
            options: CreateCompilationOptions());

        var generator = new CqsSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return (outputCompilation, diagnostics, driver.GetRunResult());
    }

    private static CSharpCompilationOptions CreateCompilationOptions()
        => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable);

    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        // Add core .NET references
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location));

        // Add System.Collections (for List<>, etc.)
        var collectionsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Collections");
        if (collectionsAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(collectionsAssembly.Location));
        }

        // Add softaware.Cqs core (IRequest, ICommand, IQuery, IRequestHandler, IRequestProcessor, NoResult)
        references.Add(MetadataReference.CreateFromFile(typeof(IRequestProcessor).Assembly.Location));

        // Add Microsoft.Extensions.DependencyInjection.Abstractions
        var diAbstractionsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Microsoft.Extensions.DependencyInjection.Abstractions");
        if (diAbstractionsAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(diAbstractionsAssembly.Location));
        }

        // Add system threading
        var threadingAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "System.Threading");
        if (threadingAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(threadingAssembly.Location));
        }

        // Add netstandard
        var netstandardAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "netstandard");
        if (netstandardAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(netstandardAssembly.Location));
        }

        return references;
    }

    /// <summary>
    /// Gets the generated source text for a specific hint name from the run result.
    /// </summary>
    public static string? GetGeneratedSource(GeneratorDriverRunResult result, string hintName)
    {
        return result.Results
            .SelectMany(r => r.GeneratedSources)
            .FirstOrDefault(s => s.HintName == hintName)
            .SourceText?.ToString();
    }
}
