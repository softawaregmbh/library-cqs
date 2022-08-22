using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.FluentValidation;
using softaware.Cqs.Decorators.Transaction;
using softaware.Cqs.Decorators.UsageAware;
using softaware.Cqs.Decorators.Validation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;
using softaware.UsageAware;

namespace softaware.Cqs.Tests;

[TestFixture]
public abstract class CancellationTokenTest : TestBase
{
    [Test]
    public async Task DoNotCancelLongRunningCommand()
    {
        await this.requestProcessor.HandleAsync(new LongRunningCommand(), default);
    }

    [Test]
    public void CancelLongRunningCommand()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        // start task
        var task = this.requestProcessor.HandleAsync(new LongRunningCommand(), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Test]
    public async Task DoNotCancelLongRunningQuery()
    {
        await this.requestProcessor.HandleAsync(new LongRunningQuery(), default);
    }

    [Test]
    public void CancelLongRunningQuery()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        // start task
        var task = this.requestProcessor.HandleAsync(new LongRunningQuery(), cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    private class SimpleInjectorTest
        : CancellationTokenTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            // Register all decorators to make sure that cancellation token is correctly passed to inner handler.
            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(TransactionAwareRequestHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(UsageAwareRequestHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(UsageAwareRequestHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(ValidationRequestHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(FluentValidationRequestHandlerDecorator<,>)));

            this.container.RegisterInstance<Cqs.Decorators.Validation.IValidator>(new DataAnnotationsValidator());
            this.container.Collection.Register(typeof(IValidator<>), Assembly.GetExecutingAssembly());
            this.container.RegisterInstance<IUsageAwareLogger>(new FakeUsageAwareLogger());
            this.container.Register(typeof(UsageAwareLogger<,>));
            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
    }

    private class ServiceCollectionTest
        : CancellationTokenTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddTransactionCommandHandlerDecorator()
                    .AddTransactionQueryHandlerDecorator()
                    .AddUsageAwareDecorators()
                    .AddDataAnnotationsValidationDecorators()
                    .AddFluentValidationDecorators(
                        builder => builder.IncludeTypesFrom(Assembly.GetExecutingAssembly())));

            services.AddSingleton<IUsageAwareLogger>(new FakeUsageAwareLogger());
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
