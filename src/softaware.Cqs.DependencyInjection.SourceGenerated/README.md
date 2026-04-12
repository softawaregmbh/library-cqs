# softaware.Cqs.DependencyInjection.SourceGenerated

A **compile-time** alternative to `softaware.Cqs.DependencyInjection` that uses a Roslyn source generator instead of Scrutor-based runtime reflection to register CQS handlers and decorators.

## Quick Start

```csharp
services
    .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyMarkerType)))
    .AddDecorators(b => b
        .AddRequestHandlerDecorator(typeof(LoggingDecorator<,>))
        .AddRequestHandlerDecorator(typeof(ValidationDecorator<,>)));
```

> **Important:** All arguments to `IncludeTypesFrom` and `AddRequestHandlerDecorator` **must** be `typeof()` expressions.  
> Variables, method calls, or other expressions will produce a compile error (`SACQS007`).

## What the Source Generator Does

At compile time, the generator:

1. Reads `AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(...)))` from the syntax tree
2. Discovers all `IRequestHandler<TRequest, TResult>` implementations in the referenced assemblies
3. Evaluates generic type constraints to determine which decorators apply to which handlers
4. Generates explicit `IServiceCollection` registrations with decorator chains
5. Generates a `GeneratedRequestProcessor` for static dispatch (registered as `IRequestProcessor`)

At runtime, the `AddSoftawareCqs()` call locates the generated `CqsServiceRegistration` class via reflection and invokes `RegisterAll(IServiceCollection)`.  
The `AddDecorators()` call is a **no-op at runtime** — the generator already read it at compile time.

## Diagnostics

| ID | Severity | Description |
|---|---|---|
| `SACQS003` | Warning | Convenience method (e.g. `AddTransactionCommandHandlerDecorator`) detected. Use `AddRequestHandlerDecorator(typeof(...))` instead. |
| `SACQS004` | Warning | No `AddSoftawareCqs` call found. The generator has nothing to generate. |
| `SACQS005` | Warning | Core CQS types (`IRequestHandler`, `IRequest`, `IRequestProcessor`) could not be resolved. Ensure `softaware.CQS` is referenced. |
| `SACQS006` | Info | Generation succeeded. Shows handler and decorator counts. Only visible in IDE Error List (with Info filter) or `dotnet build -v detailed`. |
| `SACQS007` | Error | Argument to `IncludeTypesFrom` or `AddRequestHandlerDecorator` is not a `typeof()` expression. |
| `SACQS008` | Error | Handler uses an open generic request type (e.g. `MyRequest<TEntity>`). Not supported in this version. |
| `SACQS009` | Info | `AddRequestHandlerDecorator` inside a conditional block — will use runtime registry check. |

## Conditional Decorator Registration

Unlike the runtime (Scrutor-based) package, the source generator reads **all** `AddRequestHandlerDecorator` calls from the syntax tree at compile time — including those inside `if`/`switch` blocks.

To handle this correctly, when the generator detects a decorator inside a conditional block, it generates an `if (registry.IsEnabled(...))` guard in the factory lambda. At runtime, the `AddDecorators` lambda is **actually executed** (it's not a no-op), and the `SoftawareCqsDecoratorBuilder` records which decorators were called. This information is stored in a `CqsDecoratorRegistry` singleton.

```csharp
services
    .AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyMarker)))
    .AddDecorators(b =>
    {
        if (useLogging) // ✅ This works — evaluated at runtime
        {
            b.AddRequestHandlerDecorator(typeof(LoggingDecorator<,>));
        }

        b.AddRequestHandlerDecorator(typeof(ValidationDecorator<,>)); // Always applied
    });
```

The generated code for a handler would look like:

```csharp
services.AddTransient<IRequestHandler<MyCommand, NoResult>>(sp =>
{
    var __decoratorRegistry = sp.GetRequiredService<CqsDecoratorRegistry>();
    IRequestHandler<MyCommand, NoResult> current = ActivatorUtilities.CreateInstance<MyCommandHandler>(sp);

    if (__decoratorRegistry.IsEnabled(typeof(LoggingDecorator<,>)))
    {
        current = ActivatorUtilities.CreateInstance<LoggingDecorator<MyCommand, NoResult>>(sp, current);
    }

    current = ActivatorUtilities.CreateInstance<ValidationDecorator<MyCommand, NoResult>>(sp, current);
    return current;
});
```

> **Note:** When no decorators are conditional (the common case), the registry is not used and there is zero runtime overhead.

## Debugging

### Is the generator running?

Build the project and look for warning `SACQS004`, `SACQS005`, or `SACQS006` in the build output:

```
warning SACQS006: softaware.Cqs source generator: Registered 5 handler(s) with 3 decorator(s). IRequestProcessor → GeneratedRequestProcessor
```

If you see **no SACQS warnings at all**, the generator is not running. Check:

- The NuGet package is properly installed (`dotnet list package`)
- Clear the NuGet cache: `dotnet nuget locals all --clear`
- Rebuild from scratch: `dotnet clean && dotnet build`

> **Note:** `SACQS006` is an Info-level diagnostic that only appears in Visual Studio's Error List (enable the "Messages" filter) or when building with `dotnet build -v detailed`. The warning-level diagnostics (`SACQS004`, `SACQS005`) always appear in normal build output.

### Inspect generated files

Add this to your `.csproj`:

```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

Then build and check the generated files at:

```
obj/Debug/{TFM}/generated/softaware.Cqs.DependencyInjection.SourceGenerator/softaware.Cqs.DependencyInjection.SourceGenerator.CqsSourceGenerator/
```

You should see:
- `CqsServiceRegistration.g.cs` — handler and decorator registrations
- `GeneratedRequestProcessor.g.cs` — static request dispatch

### Attach a debugger

Set the environment variable `SACQS_DEBUG_GENERATOR=1` before building:

```powershell
$env:SACQS_DEBUG_GENERATOR = "1"
dotnet build
```

This triggers `Debugger.Launch()` and lets you step through the generator in Visual Studio.

### Common pitfalls

| Symptom | Cause | Fix |
|---|---|---|
| No warnings, no generated files | Generator not running | Clear NuGet cache, rebuild |
| `SACQS004` — "No AddSoftawareCqs call found" | Missing or incorrect `AddSoftawareCqs` call | Ensure you call `services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(...)))` |
| `SACQS005` — "Core CQS types not resolved" | Missing `softaware.CQS` package reference | Add `<PackageReference Include="softaware.CQS" />` |
| `SACQS007` — "must be a typeof() expression" | Using a variable instead of `typeof()` | Replace `IncludeTypesFrom(myVariable)` with `IncludeTypesFrom(typeof(MyType))` |
| `InvalidOperationException` at runtime | Generated class not found | Ensure the NuGet package is installed and project was rebuilt |
| `SACQS008` — "open generic request type" | Handler like `MyHandler<TEntity> : IRequestHandler<MyRequest<TEntity>, int>` | Not supported. Use the runtime (Scrutor-based) package or refactor to closed generic types |

## Decorator Order

Decorators are applied in **registration order**:
- **First registered** = closest to the handler (innermost)
- **Last registered** = outermost (executed first)

```csharp
.AddDecorators(b => b
    .AddRequestHandlerDecorator(typeof(InnerDecorator<,>))     // wraps handler directly
    .AddRequestHandlerDecorator(typeof(OuterDecorator<,>)));   // wraps InnerDecorator
```

Execution order: `OuterDecorator → InnerDecorator → Handler`
