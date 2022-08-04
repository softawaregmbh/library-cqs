using System.Reflection;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.Transaction;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

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
                Assert.That(Transaction.Current!.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction!.TransactionInformation.Status == TransactionStatus.Committed;
            },
            shouldThrow: false);

        await this.requestProcessor.ExecuteAsync(command, default);

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
                Assert.That(Transaction.Current!.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction!.TransactionInformation.Status == TransactionStatus.Committed;
            },
            shouldThrow: true);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await this.requestProcessor.ExecuteAsync(command, default));
        Assert.That(exception!.Message, Is.EqualTo("We throw here for testing the rollback of transactions."));

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
                Assert.That(Transaction.Current!.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction!.TransactionInformation.Status == TransactionStatus.Committed;
            },
            shouldThrow: false);

        await this.requestProcessor.ExecuteAsync(query, default);

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
                Assert.That(Transaction.Current!.IsolationLevel, Is.EqualTo(IsolationLevel.ReadCommitted));

                Transaction.Current.TransactionCompleted += (s, e) => transactionCommitted = e.Transaction!.TransactionInformation.Status == TransactionStatus.Committed;
            },
            shouldThrow: true);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await this.requestProcessor.ExecuteAsync(query, default));
        Assert.That(exception!.Message, Is.EqualTo("We throw here for testing the rollback of transactions."));

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
                    .AddRequestHandlerDecorator(typeof(TransactionAwareRequestHandlerDecorator<,>))
                    .AddRequestHandlerDecorator(typeof(TransactionAwareCommandHandlerDecorator<,>)));

            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
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
