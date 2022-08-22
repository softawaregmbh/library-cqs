# Changelog

## All packages

### 4.0.0

#### Breaking Changes

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

## softaware.Cqs.DependencyInjection

### 1.0.3

* Fix: Update `Scrutor` to version 4.0.0 to support generic type constraints on decorators. (See also [Scrutor issue #159](https://github.com/khellang/Scrutor/issues/159))
  
### 1.0.2

* Fix: Register `DynamicQueryProcessor` and `DynamicCommandProcessor` as transient instead of singleton so that scoped dependencies can be resolved.

## softaware.Cqs.Decorators.FluentValidation

### 1.1.0

* Support list of validators for given `TCommand` and `TQuery`.

## softaware.Cqs.Decorators.FluentValidation.DependencyInjection

### 1.0.2

* Fix: Add dependency to `softaware.Cqs.Decorators.FluentValidation` version `1.1.0` to use new constructor parameters.

## softaware.Cqs

### 2.1.0

* Add `SoftawareCqsTypesBuilder` class.

## softaware.Cqs.DependencyInjection
### 1.0.1

* Fix: Add dependency to `softaware.Cqs` version `2.1.0` to have access to the `SoftawareCqsTypesBuilder` class.

## softaware.Cqs.Decorators.FluentValidation.DependencyInjection

### 1.0.1

* Fix: Add dependency to `softaware.Cqs.DependencyInjection` version `1.0.1` to have access to the `SoftawareCqsTypesBuilder` class.

## softaware.Cqs.Decorators.Transaction.DependencyInjection

### 1.0.0

* Initial release: Builder extensions

## softaware.Cqs.Decorators.UsageAware.DependencyInjection

### 1.0.0

* Initial release: Builder extensions

## softaware.Cqs.Decorators.Validation.DependencyInjection

### 1.0.0

* Initial release: Builder extensions

## softaware.Cqs.Decorators.FluentValidation.DependencyInjection

### 1.0.0

* Initial release: Builder extensions

## softaware.Cqs.DependencyInjection
### 1.0.0

* Initial release: add support for `Microsoft.Extensions.DependencyInjection` DI framework.

## softaware.Cqs.SimpleInjector
### 3.1.0

* Extension methods for the Simple Injector `Container` class to support fluent CQS builder syntax.

## softaware.Cqs.Decorators.FluentValidation

### 1.0.1

* Update Dependency to [FluentValidation](https://fluentvalidation.net/) to use Version 10 or higher.

## softaware.Cqs.SimpleInjector

### 3.0.0

#### Breaking Changes

* Major update of Simple Injector to version 5.
* If you are using the NuGet package `softaware.Cqs.Decorators.UsageAware`, the types `UsageAwareCommandLogger<>` and `UsageAwareQueryLogger<,>` must now be registered in the container, since these classes won't be resolved automatically in the new Simple Injector version any more by default. For more information, see https://simpleinjector.org/ructd.

    ```csharp
    this.container.Register(typeof(UsageAwareCommandLogger<>));
    this.container.Register(typeof(UsageAwareQueryLogger<,>));
    ```

## 2.1.0

### New Features

* Added new NuGet package `softaware.Cqs.Decorators.FluentValidation` which supports command and query decorators for using [FluentValidation](https://fluentvalidation.net/).

## 2.0.2

### Bug Fixes

* The `CancellationToken` will now correctly be passed to the inner handlers when using the Transaction-, Validation- or UsageAware decorators.

## 2.0.1
  * XML documentation for all public types has been added.

## 2.0.0

### Breaking Changes
* All packages now target `netstandard2.1`.
* It is now needed that the two decorators `softaware.Cqs.SimpleInjector.PublicCommandHandlerDecorator` and `softaware.Cqs.SimpleInjector.PublicQueryHandlerDecorator` are registered as **last** decorator in the chain when using `softaware.Cqs.SimpleInjector.DynamicCommandProcessor` and `softaware.Cqs.SimpleInjector.DynamicQueryProcessor`. This is needed so that the dynamic processors can call the correct overload of the `HandleAsync` method. Secondly this is needed when trying to call an `internal` decorator or an `internal` handler. If these two decorators are not registered, an exception will be thrown when trying to call `ExecuteAsync` on either the `ICommandProcessor` or `IQueryProcessor`. See [here](https://github.com/dotnetjunkie/solidservices/issues/21#issuecomment-382506019) for more details. 
* The package `softaware.Cqs.EntityFramework` is deprecated and no longer supported. It has been removed from this release.
* The constructor parameter of `softaware.Cqs.Decorators.Validation.DataAnnotationsValidator` has been removed as it's not needed.

### New Features
  * It is now possible to pass a `CancellationToken` when executing command handlers and query handlers. So it is now easily possible to cancel the execution of commands and queries. To use the cancellable version, implement the `HandleAsync(command, token)` default interface method and delegate to this implementation in the `HandleAsync(command)` method:

    ```csharp
      internal class LongRunningCommandHandler : ICommandHandler<LongRunningCommand>
      {
          public Task HandleAsync(LongRunningCommand command)
          {
              return this.HandleAsync(command, default);
          }
  
          public async Task HandleAsync(LongRunningCommand command, CancellationToken cancellationToken)
          {
              await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
          }
      }
    ```
    **Note:** For this to work, *all* existing Decorators must override the `HandleAsync(command, token)` method and pass the `cancellationToken` to the inner handlers.
  * Unit tests have been added.

## 2.0.0-beta

### Breaking Changes
* All packages now target `netstandard2.1`.
* It is now needed that the two decorators `softaware.Cqs.SimpleInjector.PublicCommandHandlerDecorator` and `softaware.Cqs.SimpleInjector.PublicQueryHandlerDecorator` are registered as **last** decorator in the chain when using `softaware.Cqs.SimpleInjector.DynamicCommandProcessor` and `softaware.Cqs.SimpleInjector.DynamicQueryProcessor`. This is needed so that the dynamic processors can call the correct overload of the `HandleAsync` method. Secondly this is needed when trying to call an `internal` decorator or an `internal` handler. If these two decorators are not registered, an exception will be thrown when trying to call `ExecuteAsync` on either the `ICommandProcessor` or `IQueryProcessor`. See [here](https://github.com/dotnetjunkie/solidservices/issues/21#issuecomment-382506019) for more details. 
* The package `softaware.Cqs.EntityFramework` is deprecated and no longer supported. It has been removed from this release.

### New Features
* It is now possible to pass a `CancellationToken` when executing command handlers and query handlers. So it is now easily possible to cancel the execution of commands and queries. To use the cancellable version, implement the `HandleAsync(command, token)` default interface method and delegate to this implementation in the `HandleAsync(command)` method:

  ```csharp
    internal class LongRunningCommandHandler : ICommandHandler<LongRunningCommand>
    {
        public Task HandleAsync(LongRunningCommand command)
        {
            return this.HandleAsync(command, default);
        }

        public async Task HandleAsync(LongRunningCommand command, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }
  ```

## 1.1.1

* [SourceLink](https://github.com/dotnet/sourcelink) support
* Deprecation of `softaware.Cqs.EntityFramework`. This package will be removed in the next major release.

## 1.0.1

* NuGet package metadata update

## 1.0.0

* Initial Release
