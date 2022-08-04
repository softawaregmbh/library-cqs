using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.Validation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Decorators;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

[TestFixture]
public abstract class GenericTypeConstraintsDecoratorTest : TestBase
{
    [Test]
    public async Task TestCommandWithAccessControl_DecoratorIsCalled()
    {
        var command = new AccessCheckedCommand();

        await this.requestProcessor.ExecuteAsync(command, default);

        Assert.That(command.AccessCheckHasBeenEvaluated, Is.True);
    }

    [Test]
    public async Task TestQueryWithAccessControl_DecoratorIsCalled()
    {
        var query = new AccessCheckedQuery();

        await this.requestProcessor.ExecuteAsync(query, default);

        Assert.That(query.AccessCheckHasBeenEvaluated, Is.True);
    }

    private class SeparateDecoratorsSimpleInjectorTest
        : GenericTypeConstraintsDecoratorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(AccessControlQueryHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(AccessControlCommandHandlerDecorator<,>)));

            this.container.RegisterInstance<IValidator>(new DataAnnotationsValidator());
            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();

    }

    private class SeparateDecoratorsServiceCollectionTest
        : GenericTypeConstraintsDecoratorTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(AccessControlQueryHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(AccessControlCommandHandlerDecorator<,>)));

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();

    }

    private class CommonDecoratorSimpleInjectorTest
        : GenericTypeConstraintsDecoratorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(AccessControlRequestHandlerDecorator<,>)));

            this.container.RegisterInstance<IValidator>(new DataAnnotationsValidator());
            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();

    }

    private class CommonDecoratorServiceCollectionTest
        : GenericTypeConstraintsDecoratorTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(AccessControlRequestHandlerDecorator<,>)));

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();
    }
}
