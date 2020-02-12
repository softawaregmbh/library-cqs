using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
    public class CancellationTokenTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            var fakeUsageAwareLogger = new FakeUsageAwareLogger();
            container.RegisterInstance<IUsageAwareLogger>(fakeUsageAwareLogger);
            container.RegisterInstance<IValidator>(new DataAnnotationsValidator());

            // Register all decorators to make sure that cancellation token is correctly passed to inner handler.
            this.container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(TransactionAwareQueryHandlerDecorator<,>));
            this.container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(UsageAwareQueryHandlerDecorator<,>));
            this.container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));

            this.container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionAwareCommandHandlerDecorator<>));
            this.container.RegisterDecorator(typeof(ICommandHandler<>), typeof(UsageAwareCommandHandlerDecorator<>));
            this.container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));


            this.RegisterPublicDecoratorsAndVerifyContainer();
        }

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
    }
}
