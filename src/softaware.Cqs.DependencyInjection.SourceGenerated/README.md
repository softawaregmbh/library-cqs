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
> Variables, method calls, or other expressions will produce a compile error (`CQ0008`).

## Migration Guide: from `softaware.Cqs.DependencyInjection`

### Step 1 — Replace the NuGet package

```xml
<!-- Remove -->
<PackageReference Include="softaware.CQS.DependencyInjection" Version="..." />

<!-- Add -->
<PackageReference Include="softaware.CQS.DependencyInjection.SourceGenerated" Version="..." />
```

Also replace any convenience `.DependencyInjection` decorator packages with their non-DI counterparts. Those packages depend on `softaware.CQS.DependencyInjection`, which transitively pulls **Scrutor** into your project — exactly what this package is designed to avoid. Since you will be using explicit `AddRequestHandlerDecorator(typeof(...))` calls (see Step 3), the extension methods provided by those packages are no longer needed anyway.

```xml
<!-- Remove — these transitively pull in softaware.CQS.DependencyInjection and Scrutor -->
<PackageReference Include="softaware.CQS.Decorators.Validation.DependencyInjection" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.FluentValidation.DependencyInjection" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.Transaction.DependencyInjection" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.UsageAware.DependencyInjection" Version="..." />

<!-- Add — the base packages without the DI extension methods -->
<PackageReference Include="softaware.CQS.Decorators.Validation" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.FluentValidation" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.Transaction" Version="..." />
<PackageReference Include="softaware.CQS.Decorators.UsageAware" Version="..." />
```

> **Tip:** After the migration, run `dotnet list package --include-transitive | Select-String Scrutor` to verify that Scrutor is no longer in your dependency tree.

### Step 2 — Change `IncludeTypesFrom` to use `typeof()`

The runtime package accepts an `Assembly` directly. The source generator can only work with `typeof()` expressions — a marker type from each assembly is enough.

```csharp
// Before
services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyHandler).Assembly));

// After
services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyHandler)));
```

If you were passing multiple assemblies, pass a marker type from each:

```csharp
// Before
services.AddSoftawareCqs(b => b
    .IncludeTypesFrom(typeof(SomeHandler).Assembly)
    .IncludeTypesFrom(typeof(OtherHandler).Assembly));

// After
services.AddSoftawareCqs(b => b
    .IncludeTypesFrom(typeof(SomeHandler))
    .IncludeTypesFrom(typeof(OtherHandler)));
```

### Step 3 — Replace convenience decorator methods

The source generator cannot trace through extension methods. Replace all convenience methods with explicit `AddRequestHandlerDecorator(typeof(...))` calls:

| Convenience method (remove)                             | Replace with                                                                         |
| ------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `.AddTransactionCommandHandlerDecorator()`              | `.AddRequestHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<,>))`    |
| `.AddTransactionQueryHandlerDecorator()`                | `.AddRequestHandlerDecorator(typeof(TransactionAwareQueryHandlerDecorator<,>))`      |
| `.AddDataAnnotationsValidationDecorators()`             | `.AddRequestHandlerDecorator(typeof(ValidationRequestHandlerDecorator<,>))`          |
| `.AddFluentValidationDecorators()`                      | `.AddRequestHandlerDecorator(typeof(FluentValidationRequestHandlerDecorator<,>))`    |
| `.AddUsageAwareDecorators()`                            | `.AddRequestHandlerDecorator(typeof(UsageAwareRequestHandlerDecorator<,>))`          |
| `.AddApplicationInsightsDependencyTelemetryDecorator()` | `.AddRequestHandlerDecorator(typeof(DependencyTelemetryRequestHandlerDecorator<,>))` |

A build warning (`CQ0004`) is emitted for every detected convenience method.

```csharp
// Before
.AddDecorators(b => b
    .AddTransactionCommandHandlerDecorator()
    .AddUsageAwareDecorators());

// After
.AddDecorators(b => b
    .AddRequestHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<,>))
    .AddRequestHandlerDecorator(typeof(UsageAwareRequestHandlerDecorator<,>)));
```

For the following replacing convenience methods, you must also add the required services to your service collection, since they were previously registered for you by the convenience methods:

| Convenience method (remove)                 | Add DI registration                                                                                                                                                                                                                 |
| ------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.AddDataAnnotationsValidationDecorators()` | `builder.Services.AddSingleton<IValidator>(new DataAnnotationsValidator());`                                                                                                                                                        |
| `.AddFluentValidationDecorators()`          | Use FluentValidation method to register all validators in assembly, e.g. `services.AddValidatorsFromAssemblyContaining<PersonValidator>()` (see [Docs](https://docs.fluentvalidation.net/en/latest/di.html#automatic-registration)) |
| `.AddUsageAwareDecorators()`                | `builder.Services.AddTransient(typeof(UsageAwareLogger<,>));`                                                                                                                                                                       |

### Step 4 — Rebuild

Build the project. If the generator is working correctly you will see:

```
info CQ0007: softaware.Cqs source generator: Registered 12 handler(s) with 5 decorator(s). IRequestProcessor → GeneratedRequestProcessor
```

---

## What is NOT supported

### Open generic request types

Request types with type parameters cannot be registered by the source generator, because it generates one explicit registration per concrete request type.

```csharp
// ❌ NOT supported — causes CQ0009 (error)
public class GetNextLogicalId<TEntity> : IQuery<int> { }
public class GetNextLogicalIdHandler<TEntity> : IRequestHandler<GetNextLogicalId<TEntity>, int> { }
```

**Workaround:** Keep this handler registered via the runtime package, or refactor to a non-generic design (e.g. pass a `Type` parameter or use a separate handler per entity type).

### Convenience decorator extension methods

The generator cannot statically trace extension method calls.

```csharp
// ❌ NOT supported — causes CQ0004 (warning, handler skipped)
.AddDecorators(b => b.AddTransactionCommandHandlerDecorator())
```

**Fix:** Use explicit `typeof()` calls as shown in Step 3 above.

### Custom helper methods inside `AddDecorators`

Any method call other than `AddRequestHandlerDecorator` inside the `AddDecorators` lambda is silently ignored by the source generator — even if that method internally calls `AddRequestHandlerDecorator`. The generator cannot trace through arbitrary method calls.

```csharp
// ❌ NOT supported — causes CQ0012 (warning, decorator silently skipped)
.AddDecorators(b =>
{
    b.AddRequestHandlerDecorator(typeof(LoggingDecorator<,>)); // ✅ recognized
    MyHelper.RegisterAllDecorators(b);                         // ❌ ignored
})
```

**Fix:** Register each decorator directly with `AddRequestHandlerDecorator(typeof(...))`:

```csharp
.AddDecorators(b => b
    .AddRequestHandlerDecorator(typeof(LoggingDecorator<,>))
    .AddRequestHandlerDecorator(typeof(ValidationDecorator<,>)))
```

### `IncludeTypesFrom` with anything other than `typeof()`

The generator reads types from the syntax tree — expressions that produce an `Assembly` or `Type` at runtime cannot be evaluated at compile time.

```csharp
// ❌ NOT supported — causes CQ0008 (error)
var marker = typeof(MyHandler);
services.AddSoftawareCqs(b => b.IncludeTypesFrom(marker));

// ❌ NOT supported — causes CQ0008 (error)
services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyHandler).Assembly));

// ✅ OK
services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(MyHandler)));
```

### Partially closed decorators

Decorators with only one type parameter (partially closed generics) are not supported. The runtime Scrutor package throws a `NotSupportedException` for these too, and the same restriction applies here.

```csharp
// ❌ NOT supported by either package
class Decorator<TResult> : IRequestHandler<SomeRequest, TResult> { }

// ✅ Refactor to fully generic with type constraint
class Decorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : SomeRequest { }
```

---

## What the Source Generator Does

At compile time, the generator:

1. Reads `AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(...)))` from the syntax tree
2. Discovers all `IRequestHandler<TRequest, TResult>` implementations in the referenced assemblies
3. Evaluates generic type constraints to determine which decorators apply to which handlers
4. Generates explicit `IServiceCollection` registrations with decorator chains
5. Generates a `GeneratedRequestProcessor` for static dispatch (registered as `IRequestProcessor`)

Generated code preserves nullable reference annotations on `TResult` (`Data?`, `string?`, nested forms like `List<Data?>`). Nullable value types (`int?`) were already correct and are unchanged.

```csharp
IQuery<Data?> → generates IRequestHandler<GetData, global::App.Data?>
```

At runtime, the `AddSoftawareCqs()` call locates the generated `CqsServiceRegistration` class via reflection and invokes `RegisterAll(IServiceCollection)`.  
The `AddDecorators()` lambda is **executed at runtime** to capture conditional registrations — see [Conditional Decorator Registration](#conditional-decorator-registration) below.

## Diagnostics

| ID       | Severity | Description                                                                                                                                |
| -------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| `CQ0002` | Error    | A handler is missing for the command or query.                                                                                             |
| `CQ0003` | Error    | The overload taking an assembly is not supported. Use the overload with a `typeof(HandlerMarker)` instead.                                 |
| `CQ0004` | Warning  | Convenience method (e.g. `AddTransactionCommandHandlerDecorator`) detected. Use `AddRequestHandlerDecorator(typeof(...))` instead.         |
| `CQ0005` | Warning  | No `AddSoftawareCqs` call found. The generator has nothing to generate.                                                                    |
| `CQ0006` | Warning  | Core CQS types (`IRequestHandler`, `IRequest`, `IRequestProcessor`) could not be resolved. Ensure `softaware.CQS` is referenced.           |
| `CQ0007` | Info     | Generation succeeded. Shows handler and decorator counts. Only visible in IDE Error List (with Info filter) or `dotnet build -v detailed`. |
| `CQ0008` | Error    | Argument to `IncludeTypesFrom` or `AddRequestHandlerDecorator` is not a `typeof()` expression.                                             |
| `CQ0009` | Error    | Handler uses an open generic request type (e.g. `MyRequest<TEntity>`). Not supported in this version.                                      |
| `CQ0010` | Info     | `AddRequestHandlerDecorator` inside a conditional block — will use runtime registry check.                                                 |
| `CQ0011` | Error    | Decorator has generic type parameters that could not be mapped to `TRequest`/`TResult` from `IRequestHandler<TRequest, TResult>`.          |
| `CQ0012` | Warning  | A method other than `AddRequestHandlerDecorator` was called inside `AddDecorators`. The call is ignored by the source generator.           |

## Conditional Decorator Registration

Unlike the runtime (Scrutor-based) package, the source generator reads **all** `AddRequestHandlerDecorator` calls from the syntax tree at compile time — including those inside `if`/`switch` blocks.

To handle this correctly, when the generator detects a decorator inside a conditional block, it generates an `if (registry.IsEnabled(...))` guard in the factory lambda. At runtime, the `AddDecorators` lambda is **actually executed**, and the `SoftawareCqsDecoratorBuilder` records which decorators were called. This information is stored in a `CqsDecoratorRegistry` singleton.

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

Build the project and look for warning `CQ0005` and `CQ0006`, or info `CQ0007` in the build output:

```
info CQ0007: softaware.Cqs source generator: Registered 5 handler(s) with 3 decorator(s). IRequestProcessor → GeneratedRequestProcessor
```

If you see **no CQ warnings at all**, the generator is not running. Check:

- The NuGet package is properly installed (`dotnet list package`)
- Clear the NuGet cache: `dotnet nuget locals all --clear`
- Rebuild from scratch: `dotnet clean && dotnet build`

> **Note:** `CQ0007` is an Info-level diagnostic that only appears in Visual Studio's Error List (enable the "Messages" filter) or when building with `dotnet build -v detailed`. The warning-level diagnostics (`CQ0005`, `CQ0006`) always appear in normal build output.

### Inspect generated files

You should find the two files in Visual Studio Search (<kbd>Ctrl</kbd> + <kbd>T</kbd> shortcut):

- `CqsServiceRegistration.g.cs` — handler and decorator registrations
- `GeneratedRequestProcessor.g.cs` — static request dispatch

If not, add this to your `.csproj`:

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

Add the MSBuild property `CqsDebugSourceGenerator` to your project for the duration of the debugging session:

```xml
<PropertyGroup>
    <CqsDebugSourceGenerator>true</CqsDebugSourceGenerator>
</PropertyGroup>
```

Then build the project. This triggers `Debugger.Launch()` and lets you step through the generator in Visual Studio.

> **Note:** Remove this property once you are done debugging — it causes a debugger prompt on every build.

### Common pitfalls

| Symptom                                               | Cause                                                                        | Fix                                                                                               |
| ----------------------------------------------------- | ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| No warnings, no generated files                       | Generator not running                                                        | Clear NuGet cache, rebuild                                                                        |
| `CQ0005` — "No AddSoftawareCqs call found"            | Missing or incorrect `AddSoftawareCqs` call                                  | Ensure you call `services.AddSoftawareCqs(b => b.IncludeTypesFrom(typeof(...)))`                  |
| `CQ0006` — "Core CQS types not resolved"              | Missing `softaware.CQS` package reference                                    | Add `<PackageReference Include="softaware.CQS" />`                                                |
| `CQ0008` — "must be a typeof() expression"            | Using a variable or `.Assembly` instead of `typeof()`                        | Replace `IncludeTypesFrom(myVariable)` with `IncludeTypesFrom(typeof(MyType))`                    |
| `InvalidOperationException` at runtime                | Generated class not found                                                    | Ensure the NuGet package is installed and project was rebuilt                                     |
| `CQ0009` — "open generic request type"                | Handler like `MyHandler<TEntity> : IRequestHandler<MyRequest<TEntity>, int>` | Not supported. Use the runtime (Scrutor-based) package or refactor to closed generic types        |
| `CQ0011` — "unsupported decorator generic shape"      | Decorator has extra generic parameters beyond `TRequest`/`TResult`           | Ensure all generic parameters are used by the `IRequestHandler<TRequest, TResult>` implementation |
| `CQ0012` — "unsupported method call in AddDecorators" | A helper or extension method was called inside `AddDecorators`               | Register decorators directly via `AddRequestHandlerDecorator(typeof(...))`                        |

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
