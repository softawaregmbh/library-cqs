using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;
using softaware.UsageAware;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public abstract class UsageAwareDecoratorTest : TestBase
    {
        protected abstract FakeUsageAwareLogger GetUsageAwareLogger();

        [Test]
        public void TestUsageAwareForCommands()
        {
            var command = new SimpleCommand(1);

            this.commandProcessor.ExecuteAsync(command);

            var trackedEvent = this.GetUsageAwareLogger().TrackedEvents.SingleOrDefault();
            Assert.That(trackedEvent, Is.Not.Null);
            Assert.That(trackedEvent.area, Is.EqualTo("Commands"));
            Assert.That(trackedEvent.action, Is.EqualTo("SimpleCommand"));
            Assert.That(trackedEvent.additionalProperties.Single(p => p.Key == "type").Value, Is.EqualTo("Command"));
            Assert.That(trackedEvent.additionalProperties.Any(p => p.Key == "duration"), Is.True);
        }

        [Test]
        public void TestUsageAwareForQueries()
        {
            var query = new GetSquare(4);

            this.queryProcessor.ExecuteAsync(query);

            var trackedEvent = this.GetUsageAwareLogger().TrackedEvents.SingleOrDefault();
            Assert.That(trackedEvent, Is.Not.Null);
            Assert.That(trackedEvent.area, Is.EqualTo("Queries"));
            Assert.That(trackedEvent.action, Is.EqualTo("GetSquare"));
            Assert.That(trackedEvent.additionalProperties.Single(p => p.Key == "type").Value, Is.EqualTo("Query"));
            Assert.That(trackedEvent.additionalProperties.Any(p => p.Key == "duration"), Is.True);
        }

        private class SimpleInjectorTest
            : UsageAwareDecoratorTest
        {
            private Container container;
            private FakeUsageAwareLogger fakeUsageAwareLogger;

            [SetUp]
            public override void SetUp()
            {
                this.container = new Container();

                this.container
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => b.AddUsageAwareDecorators());

                this.fakeUsageAwareLogger = new FakeUsageAwareLogger();

                this.container.RegisterInstance<IUsageAwareLogger>(this.fakeUsageAwareLogger);

                this.container.Verify();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
            protected override FakeUsageAwareLogger GetUsageAwareLogger() => this.fakeUsageAwareLogger;
        }

        private class ServiceCollectionTest
            : UsageAwareDecoratorTest
        {
            private IServiceProvider serviceProvider;
            private FakeUsageAwareLogger fakeUsageAwareLogger;

            [SetUp]
            public override void SetUp()
            {
                var services = new ServiceCollection();

                services
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => b.AddUsageAwareDecorators());

                this.fakeUsageAwareLogger = new FakeUsageAwareLogger();

                services.AddSingleton<IUsageAwareLogger>(this.fakeUsageAwareLogger);

                this.serviceProvider = services.BuildServiceProvider();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
            protected override FakeUsageAwareLogger GetUsageAwareLogger() => this.fakeUsageAwareLogger;
        }
    }
}
