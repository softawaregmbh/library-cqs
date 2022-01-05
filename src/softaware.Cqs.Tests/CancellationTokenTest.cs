using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public abstract class CancellationTokenTest : TestBase
    {
        [Test]
        public async Task DoNotCancelLongRunningCommand()
        {
            await this.commandProcessor.ExecuteAsync(new LongRunningCommand());
        }

        [Test]
        public void CancelLongRunningCommand()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            // start task
            var task = this.commandProcessor.ExecuteAsync(new LongRunningCommand(), cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
        }

        [Test]
        public async Task DoNotCancelLongRunningQuery()
        {
            await this.queryProcessor.ExecuteAsync(new LongRunningQuery());
        }

        [Test]
        public void CancelLongRunningQuery()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            // start task
            var task = this.queryProcessor.ExecuteAsync(new LongRunningQuery(), cancellationTokenSource.Token);
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
                        .AddQueryHandlerDecorator(typeof(TransactionAwareQueryHandlerDecorator<,>))
                        .AddCommandHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<>))
                        .AddQueryHandlerDecorator(typeof(UsageAwareQueryHandlerDecorator<,>))
                        .AddCommandHandlerDecorator(typeof(UsageAwareCommandHandlerDecorator<>))
                        .AddQueryHandlerDecorator(typeof(ValidationQueryHandlerDecorator<,>))
                        .AddCommandHandlerDecorator(typeof(ValidationCommandHandlerDecorator<>))
                        .AddQueryHandlerDecorator(typeof(FluentValidationQueryHandlerDecorator<,>))
                        .AddCommandHandlerDecorator(typeof(FluentValidationCommandHandlerDecorator<>)));

                this.container.RegisterInstance<Decorators.Validation.IValidator>(new DataAnnotationsValidator());
                this.container.Collection.Register(typeof(IValidator<>), Assembly.GetExecutingAssembly());
                this.container.RegisterInstance<IUsageAwareLogger>(new FakeUsageAwareLogger());
                this.container.Register(typeof(UsageAwareCommandLogger<>));
                this.container.Register(typeof(UsageAwareQueryLogger<,>));
                this.container.Register<IDependency, Dependency>();

                this.container.Verify();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
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

            protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
        }
    }
}
