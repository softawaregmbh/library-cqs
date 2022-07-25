using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.FluentValidation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

public abstract class FluentValidationDecoratorTest : TestBase
{
    protected abstract IEnumerable<IValidator<T>> GetAllValidators<T>() where T : class;

    [Test]
    public void CommandValidationSucceedsWhenEndIsAfterStart()
    {
        var command = new StartAndEndDateCommand
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        };

        var validators = this.GetAllValidators<StartAndEndDateCommand>();
        var isValid = validators.All(v => v.Validate(command).IsValid);

        Assert.IsTrue(isValid);
    }

    [Test]
    public void CommandValidationFailsWhenEndIsBeforeStart()
    {
        var command = new StartAndEndDateCommand
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(-1)
        };

        var validators = this.GetAllValidators<StartAndEndDateCommand>();
        var isValid = validators.All(v => v.Validate(command).IsValid);

        Assert.IsFalse(isValid);
    }

    [Test]
    public async Task CommandExecutionSucceedsWhenValid()
    {
        var command = new StartAndEndDateCommand
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        };

        await this.commandProcessor.ExecuteAsync(command);

        Assert.IsTrue(command.CommandExecuted);
    }

    [Test]
    public void CommandExecutionThrowsValidationExceptionWhenInvalid()
    {
        var command = new StartAndEndDateCommand
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(-1)
        };

        Assert.ThrowsAsync<ValidationException>(async () => await this.commandProcessor.ExecuteAsync(command));
    }

    [Test]
    public void QueryValidationSucceedsWhenEndIsAfterStart()
    {
        var query = new StartAndEndDate
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        };

        var validators = this.GetAllValidators<StartAndEndDate>();
        var isValid = validators.All(v => v.Validate(query).IsValid);

        Assert.IsTrue(isValid);
    }

    [Test]
    public void QueryValidationFailsWhenEndIsBeforeStart()
    {
        var query = new StartAndEndDate
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(-1)
        };

        var validators = this.GetAllValidators<StartAndEndDate>();
        var isValid = validators.All(v => v.Validate(query).IsValid);

        Assert.IsFalse(isValid);
    }

    [Test]
    public async Task QueryExecutionSucceedsWhenValid()
    {
        var query = new StartAndEndDate
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(1)
        };

        var result = await this.queryProcessor.ExecuteAsync(query);

        Assert.IsTrue(result);
    }

    [Test]
    public void QueryExecutionThrowsValidationExceptionWhenInvalid()
    {
        var query = new StartAndEndDate
        {
            Start = DateTime.Now,
            End = DateTime.Now.AddHours(-1)
        };

        Assert.ThrowsAsync<ValidationException>(async () => await this.queryProcessor.ExecuteAsync(query));
    }

    public class SimpleInjectorTest : FluentValidationDecoratorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddQueryHandlerDecorator(typeof(FluentValidationQueryHandlerDecorator<,>))
                    .AddCommandHandlerDecorator(typeof(FluentValidationCommandHandlerDecorator<>)));

            this.container.Collection.Register(typeof(IValidator<>), Assembly.GetExecutingAssembly());

            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
        protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();

        protected override IEnumerable<IValidator<T>> GetAllValidators<T>() => this.container.GetAllInstances<IValidator<T>>();
    }

    private class ServiceCollectionTest
        : FluentValidationDecoratorTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b.AddFluentValidationDecorators(
                    builder => builder.IncludeTypesFrom(Assembly.GetExecutingAssembly())));

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

        protected override IEnumerable<IValidator<T>> GetAllValidators<T>() => this.serviceProvider.GetRequiredService<IEnumerable<IValidator<T>>>();
    }
}
