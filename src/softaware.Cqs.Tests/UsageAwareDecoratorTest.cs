using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;
using softaware.UsageAware;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class UsageAwareDecoratorTest
    {
        private Container container;
        private FakeUsageAwareLogger fakeUsageAwareLogger;
        private ICommandProcessor commandProcessor;
        private IQueryProcessor queryProcessor;

        [SetUp]
        public void SetUp()
        {
            this.container = new Container();

            this.fakeUsageAwareLogger = new FakeUsageAwareLogger();

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b.AddUsageAwareDecorators());

            this.container.RegisterInstance<IUsageAwareLogger>(this.fakeUsageAwareLogger);

            this.container.Verify();

            this.commandProcessor = this.container.GetInstance<ICommandProcessor>();
            this.queryProcessor = this.container.GetInstance<IQueryProcessor>();
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
