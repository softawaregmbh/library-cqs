using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

using Test = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
    softaware.Cqs.Analyzers.IRequestShouldNotBeImplementedDirectlyAnalyzer,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

using Verify = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    softaware.Cqs.Analyzers.IRequestShouldNotBeImplementedDirectlyAnalyzer>;

namespace softaware.Cqs.Analyzers.Tests;

public class IRequestShouldNotBeImplementedDirectlyAnalyzerTests
{
    [Theory]
    [InlineData("using softaware.Cqs; public class Command : ICommand { }")]
    [InlineData("public class Command : softaware.Cqs.ICommand { }")]
    [InlineData("using softaware.Cqs; public class Command : ICommand<int> { }")]
    [InlineData("public class Command : softaware.Cqs.ICommand<int> { }")]
    [InlineData("using softaware.Cqs; public class Query : IQuery<string> { }")]
    [InlineData("public class Query : softaware.Cqs.IQuery<string> { }")]
    public async Task ImplementingSoftawareCqsInterfaces_DoesntTriggerDiagnostic(string source)
    {
        await TestAsync(source);
    }

    [Theory]
    [InlineData("class Request : {|#0:softaware.Cqs.IRequest<string>|} { }")]
    [InlineData("class Request : object, System.IDisposable, {|#0:softaware.Cqs.IRequest<string>|} { public void Dispose() { } }")]
    [InlineData("using softaware.Cqs; class Request : {|#0:IRequest<string>|} { }")]
    [InlineData("using System; using softaware.Cqs; class Request : object, IDisposable, {|#0:IRequest<string>|} { public void Dispose() { } }")]
    public async Task ImplementingIRequest_DoesTriggerDiagnostic(string source)
    {
        await TestAsync(
           source,
           Verify.Diagnostic("CQ0001").WithLocation(0).WithArguments("Request"));
    }

    private static async Task TestAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
#if DEBUG
        var softawareCqsDllPath = "../../../../softaware.Cqs/bin/Debug/netstandard2.0/softaware.Cqs.dll";
#else
        var softawareCqsDllPath = "../../../../softaware.Cqs/bin/Release/netstandard2.0/softaware.Cqs.dll";
#endif

        var test = new Test
        {
            TestState =
            {
                Sources =
                {
                    source
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(softawareCqsDllPath)
                }
            },
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

        await test.RunAsync();
    }
}
