# softaware command-query separation (CQS)

This project provides a library for the command-query separation pattern.
Commands and queries will be separated on class-level and will be represented by the `ICommand` and `IQuery<TResult>` interfaces.

The project consists of several separate packages, which allows flexible usage of various features.

| Package | NuGet | Description |
| --- | ----- | --- |
[`softaware.CQS`](src/softaware.Cqs) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS/) | Core library for command-query separation pattern. |
[`softaware.CQS.SimpleInjector`](src/softaware.Cqs.SimpleInjector) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.SimpleInjector.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.SimpleInjector/) | Adds support for dynamic resolving of commands handlers and query handlers via SimpleInjector. |
[`softaware.CQS.Decorators.Transaction`](src/softaware.Cqs.Decorators.Transaction) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Transaction.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Transaction/) | A decorator for command-query architecture, which supports transactions. |
[`softaware.CQS.Decorators.Validation`](src/softaware.Cqs.Decorators.Validation) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.Validation.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.Validation/) | A decorator for command-query architecture, which supports validation of data annotations. |
[`softaware.CQS.Decorators.UsageAware`](src/softaware.Cqs.Decorators.UsageAware) | [![NuGet](https://img.shields.io/nuget/v/softaware.CQS.Decorators.UsageAware.svg?style=flat-square)](https://www.nuget.org/packages/softaware.CQS.Decorators.UsageAware/) | A decorator for command-query architecture, which adds support for [UsageAware](https://github.com/softawaregmbh/library-usageaware). |