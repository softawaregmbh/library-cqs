using Xunit;

namespace softaware.Cqs.DependencyInjection.SourceGenerator.Tests;

public class CqsSourceGeneratorTests
{
    [Fact]
    public void BasicHandler_GeneratesRegistration()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class GetSquare : IQuery<int>
{
    public int Value { get; set; }
}

public class GetSquareHandler : IRequestHandler<GetSquare, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(GetSquare query, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(query.Value * query.Value);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(GetSquare)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs");
        var processorSource = TestHelper.GetGeneratedSource(runResult, "GeneratedRequestProcessor.g.cs");

        Assert.NotNull(registrationSource);
        Assert.NotNull(processorSource);
        Assert.Contains("GetSquareHandler", registrationSource);
        Assert.Contains("GetSquare", processorSource);
    }

    [Fact]
    public void CommandHandler_GeneratesRegistration()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class SaveThing : ICommand
{
    public string Name { get; set; }
}

public class SaveThingHandler : IRequestHandler<SaveThing, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(SaveThing command, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(SaveThing)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs");
        Assert.NotNull(registrationSource);
        Assert.Contains("SaveThingHandler", registrationSource);
        Assert.Contains("NoResult", registrationSource);
    }

    [Fact]
    public void DecoratorWithCommandConstraint_AppliesOnlyToCommands()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class MyCommand : ICommand { }
public class MyQuery : IQuery<int> { }

public class MyCommandHandler : IRequestHandler<MyCommand, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(MyCommand c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class MyQueryHandler : IRequestHandler<MyQuery, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(MyQuery q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(42);
}

public class CommandOnlyDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;
    public CommandOnlyDecorator(IRequestHandler<TRequest, TResult> decoratee) => this.decoratee = decoratee;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct)
        => this.decoratee.HandleAsync(r, ct);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyCommand)))
            .AddDecorators(b => b.AddRequestHandlerDecorator(typeof(CommandOnlyDecorator<,>)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;
        Assert.NotNull(registrationSource);

        // CommandOnlyDecorator should appear in MyCommand registration
        Assert.Contains("CommandOnlyDecorator", registrationSource);

        // But the query handler should NOT have the decorator
        // Split by handler registrations and check
        var lines = registrationSource.Split('\n');
        var querySection = false;
        var commandSection = false;
        var decoratorInQuery = false;
        var decoratorInCommand = false;

        foreach (var line in lines)
        {
            if (line.Contains("MyQueryHandler"))
                querySection = true;
            if (line.Contains("MyCommandHandler"))
                commandSection = true;
            if (line.Contains("return current;") || (line.Contains("AddTransient") && !line.Contains("IRequestProcessor")))
            {
                querySection = false;
                commandSection = false;
            }
            if (querySection && line.Contains("CommandOnlyDecorator"))
                decoratorInQuery = true;
            if (commandSection && line.Contains("CommandOnlyDecorator"))
                decoratorInCommand = true;
        }

        Assert.True(decoratorInCommand, "CommandOnlyDecorator should be applied to command handler");
        Assert.False(decoratorInQuery, "CommandOnlyDecorator should NOT be applied to query handler");
    }

    [Fact]
    public void MultipleDecorators_AppliedInCorrectOrder()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class MyCommand : ICommand { }

public class MyCommandHandler : IRequestHandler<MyCommand, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(MyCommand c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class DecoratorA<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;
    public DecoratorA(IRequestHandler<TRequest, TResult> decoratee) => this.decoratee = decoratee;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct)
        => this.decoratee.HandleAsync(r, ct);
}

public class DecoratorB<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;
    public DecoratorB(IRequestHandler<TRequest, TResult> decoratee) => this.decoratee = decoratee;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct)
        => this.decoratee.HandleAsync(r, ct);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyCommand)))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(DecoratorA<,>))
                .AddRequestHandlerDecorator(typeof(DecoratorB<,>)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;
        Assert.NotNull(registrationSource);

        // DecoratorA should appear BEFORE DecoratorB (A is closer to handler, B wraps A)
        var indexA = registrationSource.IndexOf("DecoratorA");
        var indexB = registrationSource.IndexOf("DecoratorB");
        Assert.True(indexA > 0, "DecoratorA should be in the generated code");
        Assert.True(indexB > 0, "DecoratorB should be in the generated code");
        Assert.True(indexA < indexB, "DecoratorA (closest to handler) should appear before DecoratorB (outermost)");
    }

    [Fact]
    public void DecoratorWithInterfaceConstraint_OnlyAppliesWhenRequestImplementsInterface()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public interface IAccessChecked
{
    bool Checked { get; set; }
}

public class CheckedCommand : ICommand, IAccessChecked
{
    public bool Checked { get; set; }
}

public class UncheckedCommand : ICommand { }

public class CheckedCommandHandler : IRequestHandler<CheckedCommand, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(CheckedCommand c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class UncheckedCommandHandler : IRequestHandler<UncheckedCommand, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(UncheckedCommand c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class AccessCheckDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>, IAccessChecked
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;
    public AccessCheckDecorator(IRequestHandler<TRequest, TResult> decoratee) => this.decoratee = decoratee;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct)
        => this.decoratee.HandleAsync(r, ct);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(CheckedCommand)))
            .AddDecorators(b => b.AddRequestHandlerDecorator(typeof(AccessCheckDecorator<,>)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;
        Assert.NotNull(registrationSource);

        // AccessCheckDecorator should apply to CheckedCommand but not UncheckedCommand
        Assert.Contains("AccessCheckDecorator<global::TestApp.CheckedCommand", registrationSource);
        Assert.DoesNotContain("AccessCheckDecorator<global::TestApp.UncheckedCommand", registrationSource);
    }

    [Fact]
    public void GeneratedRequestProcessor_ContainsAllHandlers()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class Cmd1 : ICommand { }
public class Cmd2 : ICommand { }
public class Query1 : IQuery<string> { }

public class Cmd1Handler : IRequestHandler<Cmd1, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(Cmd1 c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class Cmd2Handler : IRequestHandler<Cmd2, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(Cmd2 c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class Query1Handler : IRequestHandler<Query1, string>
{
    public System.Threading.Tasks.Task<string> HandleAsync(Query1 q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(""hello"");
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(Cmd1)));
    }
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var processorSource = TestHelper.GetGeneratedSource(runResult, "GeneratedRequestProcessor.g.cs")!;
        Assert.NotNull(processorSource);
        Assert.Contains("Cmd1", processorSource);
        Assert.Contains("Cmd2", processorSource);
        Assert.Contains("Query1", processorSource);
        Assert.Contains("GeneratedRequestProcessor", processorSource);
        Assert.Contains("IRequestProcessor", processorSource);
    }

    [Fact]
    public void NoConfiguration_GeneratesNothing()
    {
        var source = @"
namespace TestApp;

public class Foo { }
";

        var runResult = TestHelper.RunGenerator(source);

        Assert.Empty(runResult.Results.SelectMany(r => r.GeneratedSources));
    }

    [Fact]
    public void ConvenienceMethod_GeneratesWarning()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(Startup)))
            .AddDecorators(b => b.AddTransactionCommandHandlerDecorator());
    }
}

public static class FakeExtensions
{
    public static SoftawareCqsDecoratorBuilder AddTransactionCommandHandlerDecorator(this SoftawareCqsDecoratorBuilder b) => b;
}
";

        var (outputCompilation, diagnostics, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var generatorDiagnostics = runResult.Results
            .SelectMany(r => r.Diagnostics)
            .Where(d => d.Id == "SACQS003")
            .ToList();

        Assert.Single(generatorDiagnostics);
        Assert.Contains("AddTransactionCommandHandlerDecorator", generatorDiagnostics[0].GetMessage());
    }

    [Fact]
    public void SnapshotTest_BasicRegistration()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class GetValue : IQuery<int> { }

public class GetValueHandler : IRequestHandler<GetValue, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(GetValue q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(42);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(GetValue)));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);
        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;
        var processorSource = TestHelper.GetGeneratedSource(runResult, "GeneratedRequestProcessor.g.cs")!;

        // Snapshot assertions: verify exact structure
        Assert.Contains("namespace softaware.Cqs.Generated;", registrationSource);
        Assert.Contains("internal static class CqsServiceRegistration", registrationSource);
        Assert.Contains("public static void RegisterAll", registrationSource);
        Assert.Contains("ActivatorUtilities.CreateInstance<global::TestApp.GetValueHandler>", registrationSource);
        Assert.Contains("GeneratedRequestProcessor", registrationSource);

        Assert.Contains("namespace softaware.Cqs.Generated;", processorSource);
        Assert.Contains("internal sealed class GeneratedRequestProcessor", processorSource);
        Assert.Contains("global::softaware.Cqs.IRequestProcessor", processorSource);
        Assert.Contains("global::TestApp.GetValue", processorSource);
    }

    [Fact]
    public void SnapshotTest_WithDecoratorChain()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class DoWork : ICommand { }

public class DoWorkHandler : IRequestHandler<DoWork, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(DoWork c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class AllRequestDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> d;
    public AllRequestDecorator(IRequestHandler<TRequest, TResult> d) => this.d = d;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct) => d.HandleAsync(r, ct);
}

public class CommandDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> d;
    public CommandDecorator(IRequestHandler<TRequest, TResult> d) => this.d = d;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct) => d.HandleAsync(r, ct);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(DoWork)))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(AllRequestDecorator<,>))
                .AddRequestHandlerDecorator(typeof(CommandDecorator<,>)));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);
        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;

        // Both decorators should apply to DoWork (it's a command, so both IRequest and ICommand constraints are met)
        Assert.Contains("AllRequestDecorator<global::TestApp.DoWork", registrationSource);
        Assert.Contains("CommandDecorator<global::TestApp.DoWork", registrationSource);

        // AllRequestDecorator registered first (closer to handler), CommandDecorator second (outermost)
        var indexAll = registrationSource.IndexOf("AllRequestDecorator");
        var indexCmd = registrationSource.IndexOf("CommandDecorator");
        Assert.True(indexAll < indexCmd);
    }

    [Fact]
    public void GeneratedCode_RegistersIRequestProcessor()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class MyQuery : IQuery<int> { public int Value { get; set; } }
public class MyQueryHandler : IRequestHandler<MyQuery, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(MyQuery q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(q.Value);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyQuery)));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);
        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;

        // The generated RegisterAll method must register IRequestProcessor → GeneratedRequestProcessor
        Assert.Contains("IRequestProcessor", registrationSource);
        Assert.Contains("GeneratedRequestProcessor", registrationSource);
        Assert.Contains("AddTransient<global::softaware.Cqs.IRequestProcessor, global::softaware.Cqs.Generated.GeneratedRequestProcessor>", registrationSource);
    }

    [Fact]
    public void IncludeTypesFrom_WithVariable_GeneratesError()
    {
        var source = @"
using System;
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class MyQuery : IQuery<int> { }
public class MyQueryHandler : IRequestHandler<MyQuery, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(MyQuery q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(42);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        var markerType = typeof(MyQuery);
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(markerType));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var errors = runResult.Results
            .SelectMany(r => r.Diagnostics)
            .Where(d => d.Id == "SACQS007")
            .ToList();

        Assert.Single(errors);
        Assert.Contains("IncludeTypesFrom", errors[0].GetMessage());
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, errors[0].Severity);

        // Should NOT generate any source files when there are errors
        Assert.Empty(runResult.Results.SelectMany(r => r.GeneratedSources));
    }

    [Fact]
    public void AddRequestHandlerDecorator_WithVariable_GeneratesError()
    {
        var source = @"
using System;
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class MyCommand : ICommand { }
public class MyCommandHandler : IRequestHandler<MyCommand, NoResult>
{
    public System.Threading.Tasks.Task<NoResult> HandleAsync(MyCommand c, System.Threading.CancellationToken ct)
        => NoResult.CompletedTask;
}

public class MyDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> d;
    public MyDecorator(IRequestHandler<TRequest, TResult> d) => this.d = d;
    public System.Threading.Tasks.Task<TResult> HandleAsync(TRequest r, System.Threading.CancellationToken ct) => d.HandleAsync(r, ct);
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        var decoratorType = typeof(MyDecorator<,>);
        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyCommand)))
            .AddDecorators(b => b.AddRequestHandlerDecorator(decoratorType));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var errors = runResult.Results
            .SelectMany(r => r.Diagnostics)
            .Where(d => d.Id == "SACQS007")
            .ToList();

        Assert.Single(errors);
        Assert.Contains("AddRequestHandlerDecorator", errors[0].GetMessage());
    }

    [Fact]
    public void OpenGenericRequest_GeneratesError()
    {
        var source = @"
using softaware.Cqs;
using Microsoft.Extensions.DependencyInjection;

namespace TestApp;

public class GetNextLogicalId<TEntity> : IQuery<int> { }

public class GetNextLogicalIdHandler<TEntity> : IRequestHandler<GetNextLogicalId<TEntity>, int>
{
    public System.Threading.Tasks.Task<int> HandleAsync(GetNextLogicalId<TEntity> q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(1);
}

public class SimpleQuery : IQuery<string> { }
public class SimpleQueryHandler : IRequestHandler<SimpleQuery, string>
{
    public System.Threading.Tasks.Task<string> HandleAsync(SimpleQuery q, System.Threading.CancellationToken ct)
        => System.Threading.Tasks.Task.FromResult(""ok"");
}

public class Startup
{
    public void Configure(IServiceCollection services)
    {
        services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(SimpleQuery)));
    }
}
";

        var (_, _, runResult) = TestHelper.RunGeneratorWithCompilation(source);

        var errors = runResult.Results
            .SelectMany(r => r.Diagnostics)
            .Where(d => d.Id == "SACQS008")
            .ToList();

        Assert.Single(errors);
        Assert.Contains("GetNextLogicalIdHandler", errors[0].GetMessage());
        Assert.Contains("GetNextLogicalId", errors[0].GetMessage());

        // The non-generic handler should still be generated
        var registrationSource = TestHelper.GetGeneratedSource(runResult, "CqsServiceRegistration.g.cs")!;
        Assert.Contains("SimpleQueryHandler", registrationSource);
        Assert.DoesNotContain("GetNextLogicalIdHandler", registrationSource);
    }
}
