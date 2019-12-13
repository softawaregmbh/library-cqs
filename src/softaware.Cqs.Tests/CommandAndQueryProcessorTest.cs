using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    public class CommandAndQueryProcessorTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            this.RegisterPublicDecoratorsAndVerifyContainer();
        }

        [Test]
        public async Task ExecuteCommand()
        {
            var command = new SimpleCommand(value: 1);

            await this.commandProcessor.ExecuteAsync(command);

            Assert.That(command.Value, Is.EqualTo(2));
        }

        [Test]
        public async Task ExecuteQuery()
        {
            var query = new GetSquare(value: 4);

            var result = await this.queryProcessor.ExecuteAsync(query);

            Assert.That(result, Is.EqualTo(16));
        }
    }
}