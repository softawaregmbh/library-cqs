using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public abstract class ValidationDecoratorTest : TestBase
    {
        [Test]
        public async Task TestCommandValidation_RequiredPropertySet_Succeeds()
        {
            var command = new ValidationCommand
            {
                Value = "some value"
            };

            await this.commandProcessor.ExecuteAsync(command);
        }

        [Test]
        public void TestCommandValidation_RequiredPropertyNotSet_Throws()
        {
            var command = new ValidationCommand
            {
                Value = null
            };

            var exception = Assert.ThrowsAsync<ValidationException>(async () => await this.commandProcessor.ExecuteAsync(command));
            Assert.That(exception.Message, Is.EqualTo("The Value field is required."));
        }

        [Test]
        public async Task TestQueryValidation_PropertyInRange_Succeeds()
        {
            var query = new GetSquare(4);

            var result = await this.queryProcessor.ExecuteAsync(query);

            Assert.That(result, Is.EqualTo(16));
        }

        [Test]
        public void TestQueryValidation_PropertyOutsideRange_Throws()
        {
            var query = new GetSquare(0);

            var exception = Assert.ThrowsAsync<ValidationException>(async () => await this.queryProcessor.ExecuteAsync(query));
            Assert.That(exception.Message, Is.EqualTo("The field Value must be between 1 and 10."));
        }

        private class SimpleInjectorTest
            : ValidationDecoratorTest
        {
            private Container container;

            [SetUp]
            public override void SetUp()
            {
                this.container = new Container();

                this.container
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => b.AddDataAnnotationsValidationDecorators());

                this.container.Verify();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
        }

        private class ServiceCollectionTest
            : ValidationDecoratorTest
        {
            private IServiceProvider serviceProvider;

            [SetUp]
            public override void SetUp()
            {
                var services = new ServiceCollection();

                services
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => b.AddDataAnnotationsValidationDecorators());

                this.serviceProvider = services.BuildServiceProvider();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
        }
    }
}
