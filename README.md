# softaware command-query separation (CQS)

This project provides a library for the command-query separation pattern.
Commands and queries will be separated on class-level and will be represented by the `ICommand` and `IQuery<TResult>` interfaces.

## Usage

The software CQS packages support two dependency injection frameworks:
  1. [Dependency injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) from the `Microsoft.Extensions.DependencyInjection` package.
  2. [Simple Injector](https://simpleinjector.org/).

### Microsoft.Extensions.DependencyInjection

When using the `softaware.CQS.DependencyInjection` library some extension methods are provided to easily configure the CQS infrastructure with decorators for the `IServiceCollection`.

```csharp
var services = new ServiceCollection();

services
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
    .AddDecorators(b => b
        .AddTransactionCommandHandlerDecorator()
        .AddUsageAwareDecorators()
        .AddDataAnnotationsValidationDecorators()
        .AddFluentValidationDecorators(
            builder => builder.IncludeTypesFrom(Assembly.GetExecutingAssembly())));
```

The following NuGet packages provide extension methods for the builder to easily add decorators:
  * `softaware.Cqs.Decorators.Transaction.DependencyInjection`
  * `softaware.Cqs.Decorators.Validation.DependencyInjection`
  * `softaware.Cqs.Decorators.FluentValidation.DependencyInjection`
  * `softaware.Cqs.Decorators.UsageAware.DependencyInjection`

### SimpleInjector

When using the `softaware.CQS.SimpleInjector` library some extension methods are provided to easily configure the CQS infrastructure with decorators for the Simple Injector container:

```csharp
this.container = new Container();

this.container
    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
    .AddDecorators(b => b
        .AddCommandHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<>)));
```

Commands and queries are executed via `ICommandProcessor` and `IQueryProcessor` interfaces. They are usually injected, e.g. in an ASP.NET Core controllers. In the following example, the instances are directly resolved from the DI container:

```csharp
// using IServiceProvider
ICommandProcessor commandProcessor = serviceProvider.GetRequiredService<ICommandProcessor>();
ICommandProcessor queryProcessor = serviceProvider.GetRequiredService<IQueryProcessor>();

// or using SimpleInjector
ICommandProcessor commandProcessor = this.container.GetInstance<ICommandProcessor>();
ICommandProcessor queryProcessor = this.container.GetInstance<IQueryProcessor>();



// Execute a command without a return type.
await commandProcessor.ExecuteAsync(new SomeCommand());
// Execute a query with a return type.
var queryResult = await queryProcessor.ExecuteAsync(new SomeQuery());

```

The project consists of several separate packages, which allows flexible usage of various features.

| Package                                                                                                                              | NuGet                                                                                                                                                                                                                         | Description                                                                                                                           |
| ------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| [`softaware.CQS`](src/softaware.Cqs)                                                                                                 | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS/)                                                                                                 | Core library for command-query separation pattern.                                                                                    |
| [`softaware.CQS.SimpleInjector`](src/softaware.Cqs.SimpleInjector)                                                                   | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.SimpleInjector.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.SimpleInjector/)                                                                   | Adds support for dynamic resolving of commands handlers and query handlers via SimpleInjector.                                        |
| [`softaware.CQS.DependencyInjection`](src/softaware.Cqs.DependencyInjection)                                                         | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.DependencyInjection/)                                                         | Adds support for dynamic resolving of commands handlers and query handlers via `Microsoft.Extensions.DependencyInjection`.            |
| [`softaware.CQS.Decorators.Transaction`](src/softaware.Cqs.Decorators.Transaction)                                                   | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Transaction.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Transaction/)                                                   | A decorator for command-query architecture, which supports transactions.                                                              |
| [`softaware.CQS.Decorators.Transaction.DependencyInjection`](src/softaware.Cqs.Decorators.Transaction.DependencyInjection)           | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Transaction.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Transaction.DependencyInjection/)           | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.Validation`](src/softaware.Cqs.Decorators.Validation)                                                     | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Validation.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Validation/)                                                     | A decorator for command-query architecture, which supports validation of data annotations.                                            |
| [`softaware.CQS.Decorators.Validation.DependencyInjection`](src/softaware.Cqs.Decorators.Validation.DependencyInjection)             | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Validation.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Validation.DependencyInjection/)             | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.FluentValidation`](src/softaware.Cqs.Decorators.FluentValidation)                                         | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.FluentValidation.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.FluentValidation/)                                         | A decorator for command-query architecture, which supports validation using [FluentValidation](https://fluentvalidation.net/).        |
| [`softaware.CQS.Decorators.FluentValidation.DependencyInjection`](src/softaware.Cqs.Decorators.FluentValidation.DependencyInjection) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.FluentValidation.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.FluentValidation.DependencyInjection/) | Builder extensions for adding decorators to Microsoft's DI.                                                                           |
| [`softaware.CQS.Decorators.UsageAware`](src/softaware.Cqs.Decorators.UsageAware)                                                     | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.UsageAware.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.UsageAware/)                                                     | A decorator for command-query architecture, which adds support for [UsageAware](https://github.com/softawaregmbh/library-usageaware). |
| [`softaware.CQS.Decorators.UsageAware.DependencyInjection`](src/softaware.Cqs.Decorators.UsageAware.DependencyInjection)             | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.UsageAware.DependencyInjection.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.UsageAware.DependencyInjection/)             | Builder extensions for adding decorators to Microsoft's DI.                                                                           |