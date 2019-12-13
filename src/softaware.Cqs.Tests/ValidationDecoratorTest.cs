using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using softaware.Cqs.Decorators.Validation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public class ValidationDecoratorTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            container.RegisterInstance<IValidator>(new DataAnnotationsValidator(container));
            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(ValidationQueryHandlerDecorator<,>));

            this.RegisterPublicDecoratorsAndVerifyContainer();
        }

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
    }
}
