using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.Decorators;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

[TestFixture]
public class GenericTypeArgumentDecoratorSimpleInjectorTests
{
    [Test]
    public void InvalidDecoratorThatDoesNotImplementIRequestHandler_Throws()
    {
        var container = new Container();

        var exception = Assert.Throws<ArgumentException>(() =>
            container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorThatDoesNotImplementIRequestHandler))));

        Assert.AreEqual(
            "The supplied type InvalidDecoratorThatDoesNotImplementIRequestHandler does not implement IRequestHandler<TRequest, TResult>. (Parameter 'serviceType')",
            exception!.Message);
    }

    [Test]
    public void InvalidDecoratorWithoutConstructor_Throws()
    {
        var container = new Container();

        var exception = Assert.Throws<ArgumentException>(() =>
            container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorWithoutConstructor<,>))));

        Assert.AreEqual(
            "For the container to be able to use InvalidDecoratorWithoutConstructor<TRequest, TResult> as a decorator, its constructor must include a single parameter of type IRequestHandler<TRequest, TResult> (or Func<IRequestHandler<TRequest, TResult>>) - i.e. the type of the instance that is being decorated. The parameter type IRequestHandler<TRequest, TResult> does not currently exist in the constructor of class InvalidDecoratorWithoutConstructor<TRequest, TResult>. (Parameter 'decoratorType')",
            exception!.Message);
    }

    [Test]
    public void InvalidDecoratorWithWrongConstructorParameter_Throws()
    {
        var container = new Container();

        var exception = Assert.Throws<ArgumentException>(() =>
            container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorWithWrongConstructorParameter<>))));

        Assert.AreEqual(
            "For the container to be able to use InvalidDecoratorWithWrongConstructorParameter<TRequest> as a decorator, its constructor must include a single parameter of type IRequestHandler<TRequest, int> (or Func<IRequestHandler<TRequest, int>>) - i.e. the type of the instance that is being decorated. The parameter type IRequestHandler<TRequest, int> does not currently exist in the constructor of class InvalidDecoratorWithWrongConstructorParameter<TRequest>. (Parameter 'decoratorType')",
            exception!.Message);
    }

    [Test]
    public async Task ValidDecoratorWithGenericArguments_Works()
    {
        var container = new Container();

        container
            .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(ValidDecoratorWithGenericArguments<,>)));

        container.Register<IDependency, Dependency>();

        container.Verify();

        var requestProcessor = container.GetRequiredService<IRequestProcessor>();

        var command = new SimpleCommand(0);
        await requestProcessor.HandleAsync(command, default);

        Assert.AreEqual(2, command.Value);
    }

    [Test]
    public async Task PartiallyClosedGenericDecorator_WorksWithSimpleInjector()
    {
        var container = new Container();

        container
            .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(PartiallyClosedGenericDecorator<>)));

        container.Register<IDependency, Dependency>();

        container.Verify();

        var requestProcessor = container.GetRequiredService<IRequestProcessor>();

        var command = new SimpleCommand(0);
        await requestProcessor.HandleAsync(command, default);

        Assert.AreEqual(2, command.Value);
    }

    [Test]
    public async Task ValidDecoratorWithoutGenericArguments_Works()
    {
        var container = new Container();

        container
            .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(ValidDecoratorWithoutGenericArguments)));

        container.Register<IDependency, Dependency>();

        container.Verify();

        var requestProcessor = container.GetRequiredService<IRequestProcessor>();

        var command = new SimpleCommand(0);
        await requestProcessor.HandleAsync(command, default);

        Assert.AreEqual(2, command.Value);
    }
}

[TestFixture]
public class GenericTypeArgumentDecoratorServiceCollectionTests
{
    [Test]
    public void InvalidDecoratorThatDoesNotImplementIRequestHandler_Throws()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorThatDoesNotImplementIRequestHandler))));

        Assert.AreEqual(
            "Type 'softaware.Cqs.Tests.Decorators.InvalidDecoratorThatDoesNotImplementIRequestHandler' cannot be used as decorator because it does not implement IRequestHandler<TRequest, TResult>. (Parameter 'decoratorType')",
            exception!.Message);
    }

    [Test]
    public void InvalidDecoratorWithoutConstructor_Throws()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorWithoutConstructor<,>))));

        Assert.AreEqual(
            "Type 'softaware.Cqs.Tests.Decorators.InvalidDecoratorWithoutConstructor`2[TRequest,TResult]' cannot be used as decorator for 'softaware.Cqs.IRequestHandler`2[TRequest,TResult]' because it has no constructor parameter with this type. (Parameter 'decoratorType')",
            exception!.Message);
    }

    [Test]
    public void InvalidDecoratorWithWrongConstructorParameter_Throws()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(InvalidDecoratorWithWrongConstructorParameter<>))));

        Assert.AreEqual(
            "Type 'softaware.Cqs.Tests.Decorators.InvalidDecoratorWithWrongConstructorParameter`1[TRequest]' cannot be used as decorator for 'softaware.Cqs.IRequestHandler`2[TRequest,System.Int32]' because it has no constructor parameter with this type. (Parameter 'decoratorType')",
            exception!.Message);
    }

    [Test]
    public async Task ValidDecoratorWithGenericArguments_Works()
    {
        var services = new ServiceCollection();

        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(ValidDecoratorWithGenericArguments<,>)));

        services.AddTransient<IDependency, Dependency>();

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var requestProcessor = serviceProvider.GetRequiredService<IRequestProcessor>();

        var command = new SimpleCommand(0);
        await requestProcessor.HandleAsync(command, default);

        Assert.AreEqual(2, command.Value);
    }

    [Test]
    public void PartiallyClosedGenericDecorator_IsNotSupportedWithMSDI()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<NotSupportedException>(() =>
            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(PartiallyClosedGenericDecorator<>))));

        var expectedMessage =
@"Sorry, partially closed decorators are not supported at the moment. You can achieve the same behavior by changing your decorator class definition to a generic type with two type arguments and applying type constraints:

For example:

class Decorator<TResult> : IRequestHandler<SomeRequest, TResult>
    where TRequest : IRequest<TResult>

can be changed to

class Decorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : SomeRequest

Alternatively, you can use softaware.CQS.SimpleInjector instead of softaware.CQS.DependencyInjection.";

        Assert.AreEqual(expectedMessage, exception!.Message);
    }

    [Test]
    public async Task ValidDecoratorWithoutGenericArguments_Works()
    {
        var services = new ServiceCollection();

        services
            .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
            .AddDecorators(b => b
                .AddRequestHandlerDecorator(typeof(ValidDecoratorWithoutGenericArguments)));

        services.AddTransient<IDependency, Dependency>();

        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        var requestProcessor = serviceProvider.GetRequiredService<IRequestProcessor>();

        var command = new SimpleCommand(0);
        await requestProcessor.HandleAsync(command, default);

        Assert.AreEqual(2, command.Value);
    }
}
