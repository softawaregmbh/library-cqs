# Changelog

## softaware.Cqs.SimpleInjector

### 3.0.0

#### Breaking Changes

* Major update of SimpleInjector to version 5.

## softaware.Cqs.Decorators.UsageAware

### 3.0.0

#### Breaking Changes

* Refactoring which was required due to update to SimpleInjector v5. It is now needed to register the `UsageAwareCommandLogger` and `UsageAwareQueryLogger` as Singletons in the container. In previous versions of SimpleInjector, these classes were resolved automatically. With the new default value for the SimpleInjector option `ResolveUnregisteredConcreteTypes ` this is no longer supported (see https://simpleinjector.org/ructd for details).

    ```csharp
    this.container.RegisterSingleton<UsageAwareCommandLogger>();
    this.container.RegisterSingleton<UsageAwareQueryLogger>();
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
