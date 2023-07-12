# softaware command-query separation (CQS)

This project provides a library for the command-query separation pattern.
Commands and queries will be separated on class-level and will be represented by the `ICommand` and `IQuery<TResult>` interfaces.

## Usage

### Queries and Commands

Define queries and commands in your application as classes/records implementing the `IQuery<TResult>` or `ICommand` interface. Both interfaces extend `IRequest<TResult>`, but differentiating between queries and commands allows us to apply decorators for cross-cutting concerns to only queries or only commands (for example, you might want to surround all command executions with a transaction). Therefore you shouldn't directly implement `IRequest<TResult>`:

```csharp
public class GetThings : IQuery<IReadOnlyCollection<Thing>>;

public record SaveThing(Thing thing) : ICommand;
```

When you have defined your queries and commands, the next step is to define the according handlers by implementing the `IRequestHandler<TRequest, TResult>` interface:

```csharp
// Query Handler
public class GetThingsHandler : IRequestHandler<GetThings, IReadOnlyCollection<Thing>>
{
    public async Task<IReadOnlyCollection<Thing>> HandleAsync(GetThings query, CancellationToken cancellationToken)
    {
      // return things
    }
}

// Command Handler
public class SaveThingHandler : IRequestHandler<SaveThing, NoResult>
{
    public async Task<NoResult> HandleAsync(SaveThing command, CancellationToken cancellationToken)
    {
        // save thing
        return NoResult.CompletedTask;
    }
}
```

A cross-cutting concern can be implemented with a decorator that also implements `IRequestHandler<TRequest, TResult>`:

```csharp
// This decorator should wrap all commands, but you can also implement
// more specific decorators by adding additional generic constraints.
public class CommandLoggingDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    public CommandLoggingDecorator(IRequestHandler<TRequest, TResult> decoratee) =>
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));

    public async Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        object type = typeof(TRequest).Name;
        Debug.WriteLine("Executing command \"{0}\"", type);

        var result = await this.decoratee.HandleAsync(request, cancellationToken);

        Debug.WriteLine("Executed command \"{0}\"", type);
        return result;
    }
}
```

It is possible to define decorators for specific query or command types (`IRequestHandler<SaveThing, NoResult>`) or generic ones that apply to all request types (`IRequestHandler<TRequest, TResult>`) or a subset (`IRequestHandler<TRequest, TResult> where TRequest : IMyMarkerInterface`). The example above applies to all commands, but not to queries.

### Dependency Injection

The software CQS packages support two dependency injection frameworks:
  1. [Dependency injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) from the `Microsoft.Extensions.DependencyInjection` package (recommended).
  2. [Simple Injector](https://simpleinjector.org/).

#### Microsoft.Extensions.DependencyInjection

When using the `softaware.CQS.DependencyInjection` library some extension methods are provided to easily configure the CQS infrastructure with decorators for the `IServiceCollection`.

```csharp
var services = new ServiceCollection();

services
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly())) // this registers all request handlers
    .AddDecorators(b => b
        .AddRequestHandlerDecorator(typeof(CommandLoggingDecorator<,>)); // this registers the CommandLoggingDecorator for all command types
```

The following NuGet packages provide extension methods for the builder to easily add some predefined decorators:
  * `softaware.Cqs.Decorators.Transaction.DependencyInjection`
  * `softaware.Cqs.Decorators.Validation.DependencyInjection`
  * `softaware.Cqs.Decorators.FluentValidation.DependencyInjection`
  * `softaware.Cqs.Decorators.UsageAware.DependencyInjection`

```csharp
var services = new ServiceCollection();

services
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
    .AddDecorators(b => b
        .AddRequestHandlerDecorator(typeof(CommandLoggingDecorator<,>))
        .AddTransactionCommandHandlerDecorator()
        .AddUsageAwareDecorators()
        .AddDataAnnotationsValidationDecorators()
        .AddFluentValidationDecorators(
            builder => builder.IncludeTypesFrom(Assembly.GetExecutingAssembly())));
```

The decorators wrap the handler in the order they are added here (so they are called in opposite order). In this case, the `FluentValidationRequestHandlerDecorator` is the first decorator to be called. The `CommandLoggingDecorator` is the last one and calls the actual handler.

#### SimpleInjector

When using the `softaware.CQS.SimpleInjector` library, use `AddRequestHandlerDecorator` method to register decorators in the Simple Injector container:

```csharp
this.container = new Container();

this.container
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
    .AddDecorators(b => b
        .AddRequestHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<>)));
```

##### Validation Decorators

To use validation decorators from (1) `softaware.CQS.Decorators.Validation` or (2) `softaware.CQS.Decorators.FluentValidation` with SimpleInjector configure it like following:

```csharp
// (1) Register validator
container.RegisterInstance<IValidator>(new DataAnnotationsValidator());

// (2) Register all fluent validators which are available in this project.
container.Collection.Register(typeof(FluentValidation.IValidator<>), Assembly.GetExecutingAssembly());

container
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
    .AddDecorators(b => b
      // (1) Add Validation Decorator
      .AddRequestHandlerDecorator(typeof(ValidationRequestHandlerDecorator<,>))
      // (2) Add FluentValidation Decorator
      .AddRequestHandlerDecorator(typeof(FluentValidationRequestHandlerDecorator<,>));
```

### Executing Commands/Queries

Commands and queries can be executed via injecting the according `IRequestHandler<TRequest, TResult>` into the caller. You will receive the actual handler wrapped by any decorators that apply.

Executing requests this way requires some boilerplate code (specifying all the generic type arguments), so the library also provides a simpler way via the `IRequestProcessor` interface. You can pass any request to it and it takes care of resolving the correct handler(s):

```csharp
// using IServiceProvider
IRequestProcessor requestProcessor = serviceProvider.GetRequiredService<IRequestProcessor>();

// or using SimpleInjector
IRequestProcessor requestProcessor = this.container.GetInstance<IRequestProcessor>();

// Execute a command without a return type.
await requestProcessor.HandleAsync(new SaveThing(thing), cancellationToken);

// Execute a query with a return type.
var queryResult = await requestProcessor.HandleAsync(new GetThings(), cancellationToken);
```

## Packages
The project consists of several separate packages, which allows flexible usage of various features.

| Package                                                                                                                                    | NuGet                                                                                                                                                                                                                               | Description                                                                                                                           |
| ------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| [`softaware.CQS`](src/softaware.Cqs)                                                                                                       | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS/)                                                                                                       | Core library for command-query separation pattern.                                                                                    |
| [`softaware.CQS.Analyzers`](src/softaware.Cqs.Analyzers)                                                                                   |                                                                                                                                                                                                                                     | Roslyn analyzers that ensure correct usage of the library. (Shipped with core library.)                                               |
| [`softaware.CQS.SimpleInjector`](src/softaware.Cqs.SimpleInjector)                                                                         | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.SimpleInjector.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.SimpleInjector/)                                                                         | Adds support for dynamic resolving of commands handlers and query handlers via SimpleInjector.                                        |
| [`softaware.CQS.DependencyInjection`](src/softaware.Cqs.DependencyInjection)                                                               | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.DependencyInjection/)                                                               | Adds support for dynamic resolving of commands handlers and query handlers via `Microsoft.Extensions.DependencyInjection`.            |
| [`softaware.CQS.Decorators.Transaction`](src/softaware.Cqs.Decorators.Transaction)                                                         | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Transaction.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Transaction/)                                                         | A decorator for command-query architecture, which supports transactions.                                                              |
| [`softaware.CQS.Decorators.Transaction.DependencyInjection`](src/softaware.Cqs.Decorators.Transaction.DependencyInjection)                 | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Transaction.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Transaction.DependencyInjection/)                 | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.Validation`](src/softaware.Cqs.Decorators.Validation)                                                           | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Validation.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Validation/)                                                           | A decorator for command-query architecture, which supports validation of data annotations.                                            |
| [`softaware.CQS.Decorators.Validation.DependencyInjection`](src/softaware.Cqs.Decorators.Validation.DependencyInjection)                   | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Validation.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Validation.DependencyInjection/)                   | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.FluentValidation`](src/softaware.Cqs.Decorators.FluentValidation)                                               | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.FluentValidation.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.FluentValidation/)                                               | A decorator for command-query architecture, which supports validation using [FluentValidation](https://fluentvalidation.net/).        |
| [`softaware.CQS.Decorators.FluentValidation.DependencyInjection`](src/softaware.Cqs.Decorators.FluentValidation.DependencyInjection)       | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.FluentValidation.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.FluentValidation.DependencyInjection/)       | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.UsageAware`](src/softaware.Cqs.Decorators.UsageAware)                                                           | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.UsageAware.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.UsageAware/)                                                           | A decorator for command-query architecture, which adds support for [UsageAware](https://github.com/softawaregmbh/library-usageaware). |
| [`softaware.CQS.Decorators.UsageAware.DependencyInjection`](src/softaware.Cqs.Decorators.UsageAware.DependencyInjection)                   | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.UsageAware.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.UsageAware.DependencyInjection/)                   | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.ApplicationInsights`](src/softaware.Cqs.Decorators.ApplicationInsights)                                         | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.ApplicationInsights.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.ApplicationInsights/)                                         | A decorator for command-query architecture, which adds support for Azure Application Insights.                                        |
| [`softaware.CQS.Decorators.ApplicationInsights.DependencyInjection`](src/softaware.Cqs.Decorators.ApplicationInsights.DependencyInjection) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.ApplicationInsights.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.ApplicationInsights.DependencyInjection/) | Builder extensions for adding decorators to Microsoft's DI.                                                                           |

## Breaking changes in version 4.0

Version 4.0 contains some breaking changes you need to be aware of before updating:

* There is now an interface `ICommand<TResult>` for commands that return values. While this should be used sparingly, there are some cases where it can be useful.
* `ICommand` (which should be the default for implementing commands) derives from this new `ICommand<NoResult>` interface.
* There is now a common base interface for `IQuery<TResult>` and `ICommand<TResult>` (and thus `ICommand`): `IRequest<TResult`. This has the following advantages:
  * There is no need to distinguish between `IQueryHandler<TResult>` and `ICommandHandler` anymore. Simply use `IRequestHandler<TResult>`.
  * For cross-cutting concerns, you can write decorators that target `IRequestHandler<TResult>`. Before you had to write one for `IQueryHandler<TResult>` and one for `ICommandHandler`.
  * Instead of `IQueryProcessor` and `ICommandProcessor`, you can simply use `IRequestProcessor`. `ExecuteAsync` has been renamed to `HandleAsync`.
* The cancellation token is now a required parameter for the `HandleAsync` method.

<details>
  <summary>Show detailed upgrade instructions</summary>
  
  * Update all `softaware.CQS` packages to version 4.0.0 or higher
  * Replace `IQueryHandler<TQuery, TResult>` with `IRequestHandler<TQuery, TResult>`:
    * Replace in files (Use regular expression)
      ```csharp
      IQueryHandler<(.*?), (.*?)>
      IRequestHandler<$1, $2>
      ```
  * Replace `ICommandHandler<TCommand>` with `IRequestHandler<TCommand, NoResult>`
    * Replace
      ```csharp
      ICommandHandler<(.*?)>
      IRequestHandler<$1, NoResult>
      ```
  
  * Replace query handler `HandleAsync` interface implementation: Add `CancellationToken`
    * Replace
      ```csharp
      Task<(.+?)> HandleAsync\(([^,]+?) ([^,]+?)\)
      Task<$1> HandleAsync($2 $3, System.Threading.CancellationToken cancellationToken)
      ```
	
  * Replace command handler `HandleAsync`: add `NoResult` and `CancellationToken`
    * Replace
      ```csharp
      Task HandleAsync\(([^,]+?) ([^,]+?)\)
      Task<NoResult> HandleAsync($1 $2, System.Threading.CancellationToken cancellationToken)
      ```

  * Remove `HandleAsync` overloads delegating to `CancellationToken` version
    * Replace with empty string (You might need to adjust the expressions based on your formatting):
      ```csharp
      Task<(.+?)> HandleAsync\(([^,]+?) ([^,]+?)\) =>
      ```
    * Replace with empty string
      ```csharp
      public  this.HandleAsync(query, default);
      ```

  * Replace `IQueryProcessor` and `ICommandProcessor` with `IRequestProcessor`
    * Replace `IQueryProcessor` with `IRequestProcessor`
    * Replace `ICommandProcessor` with `IRequestProcessor`
    * Replace `queryProcessor` with `requestProcessor`
    * Replace `commandProcessor` with `requestProcessor`
    * Replace
      ```csharp
      requestProcessor.ExecuteAsync\(([^,]+?)\);
      requestProcessor.HandleAsync($1, cancellationToken);
      ```
    * Remove duplicates where `IQueryProcessor` and `ICommandProcessor` were injected
  
  * Add `return NoResult.Value` to command handlers

  * Optional: Add `CancellationToken` to Controller actions
	  * Replace with file pattern: *Controller.cs
      * With parameters:
        ```csharp
        public async (.+)\((.+)\)
        public async $1($2, System.Threading.CancellationToken cancellationToken)
        ```
	
	    * Without parameters:
        ```csharp
        public async (.+)\(\)
        public async $1(System.Threading.CancellationToken cancellationToken)
        ```

  * Add missing `CancellationToken` parameters
  * Decorators: Refactor command handler decorators to 2 generic type parameters
  * Replace `AddQueryHandlerDecorator` and `AddCommandHandlerDecorator` with `AddRequestHandlerDecorator`
  * Remove `PublicQueryHandlerDecorator` and `PublicCommandHandlerDecorator` if you referenced them explicitely.
  * Optional: Combine duplicate decorators implementing `IQueryHandler<TResult>` and `ICommandHandler` into a single class implementing `IRequestHandler`
  * Optional: Use `ICommand<TResult>` instead of `ICommand` if you used some workarounds for returning values from commands (like setting a property on the command)
</details>