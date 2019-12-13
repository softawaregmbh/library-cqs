using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using softaware.Cqs.Decorators.UsageAware;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;
using softaware.UsageAware;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class UsageAwareDecoratorTest : TestBase
    {
        private FakeUsageAwareLogger fakeUsageAwareLogger;

        public override void SetUp()
        {
            base.SetUp();

            this.fakeUsageAwareLogger = new FakeUsageAwareLogger();

            container.RegisterInstance<IUsageAwareLogger>(this.fakeUsageAwareLogger);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(UsageAwareCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(UsageAwareQueryHandlerDecorator<,>));

            container.Verify();
        }

        [Test]
        public void TestUsageAwareForCommands()
        {
            var command = new SimpleCommand(1);

            this.commandProcessor.ExecuteAsync(command);

            var trackedEvent = this.fakeUsageAwareLogger.TrackedEvents.SingleOrDefault();
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

            var trackedEvent = this.fakeUsageAwareLogger.TrackedEvents.SingleOrDefault();
            Assert.That(trackedEvent, Is.Not.Null);
            Assert.That(trackedEvent.area, Is.EqualTo("Queries"));
            Assert.That(trackedEvent.action, Is.EqualTo("GetSquare"));
            Assert.That(trackedEvent.additionalProperties.Single(p => p.Key == "type").Value, Is.EqualTo("Query"));
            Assert.That(trackedEvent.additionalProperties.Any(p => p.Key == "duration"), Is.True);
        }
    }
}
