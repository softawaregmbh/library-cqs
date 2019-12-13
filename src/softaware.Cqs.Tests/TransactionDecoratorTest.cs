using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using softaware.Cqs.Decorators.Transaction;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class TransactionDecoratorTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(TransactionAwareCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(TransactionAwareQueryHandlerDecorator<,>));

            container.Verify();
        }

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
    }
}
