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

        await this.commandProcessor.ExecuteAsync(command);

        Assert.That(command.AccessCheckHasBeenEvaluated, Is.True);
    }

    [Test]
    public async Task TestQueryWithAccessControl_DecoratorIsCalled()
    {
        var query = new AccessCheckedQuery();

        await this.queryProcessor.ExecuteAsync(query);

        Assert.That(query.AccessCheckHasBeenEvaluated, Is.True);
    }

    private class SimpleInjectorTest
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
                    .AddQueryHandlerDecorator(typeof(AccessControlQueryHandlerDecorator<,>))
                    .AddCommandHandlerDecorator(typeof(AccessControlCommandHandlerDecorator<>)));

            this.container.RegisterInstance<IValidator>(new DataAnnotationsValidator());
            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
        protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
    }

    private class ServiceCollectionTest
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
                    .AddQueryHandlerDecorator(typeof(AccessControlQueryHandlerDecorator<,>))
                    .AddCommandHandlerDecorator(typeof(AccessControlCommandHandlerDecorator<>)));

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
        protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
    }
}
