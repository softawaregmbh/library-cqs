using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;
using softaware.UsageAware;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class CancellationTokenTest
    {
        private Container container;
        private ICommandProcessor commandProcessor;
        private IQueryProcessor queryProcessor;

        [SetUp]
        public void SetUp()
        {
            this.container = new Container();

            // Register all decorators to make sure that cancellation token is correctly passed to inner handler.
            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddTransactionQueryHandlerDecorator()
                    .AddTransactionCommandHandlerDecorator()
                    .AddUsageAwareDecorators()
                    .AddDataAnnotationsValidationDecorators()
                    .AddFluentValidationDecorators(x => x.IncludeTypesFrom(Assembly.GetExecutingAssembly())));

            this.container.RegisterInstance<IUsageAwareLogger>(new FakeUsageAwareLogger());

            this.container.Verify();

            this.commandProcessor = this.container.GetInstance<ICommandProcessor>();
            this.queryProcessor = this.container.GetInstance<IQueryProcessor>();
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
