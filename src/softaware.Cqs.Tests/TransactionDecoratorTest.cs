using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public abstract class TransactionDecoratorTest : TestBase
    {
        [Test]
        public async Task TestCommandTransaction_CommandDoesNotThrow_CommitsTransaction()
        {
            var transactionCommitted = false;

            var command = new CallbackCommand(
                () =>
                {
                    Assert.That(Transaction.Current, Is.Not.Null);
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                    Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction.TransactionInformation.Status == TransactionStatus.Committed;
                },
                shouldThrow: false);

            await this.commandProcessor.ExecuteAsync(command);

            Assert.That(transactionCommitted, Is.True);
        }

        [Test]
        public void TestCommandTransaction_CommandThrows_RollsBackTransaction()
        {
            var transactionCommitted = false;

            var command = new CallbackCommand(
                () =>
                {
                    Assert.That(Transaction.Current, Is.Not.Null);
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                    Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction.TransactionInformation.Status == TransactionStatus.Committed;
                },
                shouldThrow: true);

            var exception = Assert.ThrowsAsync<Exception>(async () => await this.commandProcessor.ExecuteAsync(command));
            Assert.That(exception.Message, Is.EqualTo("We throw here for testing the rollback of transactions."));

            Assert.That(transactionCommitted, Is.False);
        }

        [Test]
        public async Task TestQueryTransaction_QueryDoesNotThrow_CommitsTransaction()
        {
            var transactionCommitted = false;

            var query = new CallbackQuery(
                () =>
                {
                    Assert.That(Transaction.Current, Is.Not.Null);
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                    Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction.TransactionInformation.Status == TransactionStatus.Committed;
                },
                shouldThrow: false);

            await this.queryProcessor.ExecuteAsync(query);

            Assert.That(transactionCommitted, Is.True);
        }

        [Test]
        public void TestQueryTransaction_QueryThrows_RollsBackTransaction()
        {
            var transactionCommitted = false;

            var query = new CallbackQuery(
                () =>
                {
                    Assert.That(Transaction.Current, Is.Not.Null);
                    Assert.That(Transaction.Current.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                    Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction.TransactionInformation.Status == TransactionStatus.Committed;
                },
                shouldThrow: true);

            var exception = Assert.ThrowsAsync<Exception>(async () => await this.queryProcessor.ExecuteAsync(query));
            Assert.That(exception.Message, Is.EqualTo("We throw here for testing the rollback of transactions."));

            Assert.That(transactionCommitted, Is.False);
        }

        private class SimpleInjectorTest
            : TransactionDecoratorTest
        {
            private Container container;

            [SetUp]
            public override void SetUp()
            {
                this.container = new Container();

                this.container
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => b
                        .AddTransactionCommandHandlerDecorator()
                        .AddTransactionQueryHandlerDecorator());

                this.container.Verify();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
        }

        private class ServiceCollectionTest
            : TransactionDecoratorTest
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
                        .AddTransactionQueryHandlerDecorator());

                this.serviceProvider = services.BuildServiceProvider();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
        }
    }
}
