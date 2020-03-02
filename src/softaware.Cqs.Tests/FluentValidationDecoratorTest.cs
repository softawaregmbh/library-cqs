using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using NUnit.Framework;
using softaware.Cqs.Decorators.FluentValidation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Commands.Validation;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.CQ.Contract.Queries.Validation;

namespace softaware.Cqs.Tests
{
    public class FluentValidationDecoratorTest : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            // Register all fluent validators which are available in this project (e.g. StartAndEndDateCommandValidator).
            this.container.Register(typeof(IValidator<>), new[] { Assembly.GetExecutingAssembly() });

            // Register "empty" validator if no other matching validator exists.
            this.container.RegisterConditional(typeof(IValidator<>), typeof(NullValidator<>), c => !c.Handled);

            container.RegisterDecorator(typeof(ICommandHandler<>), typeof(FluentValidationCommandHandlerDecorator<>));
            container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(FluentValidationQueryHandlerDecorator<,>));

            this.RegisterPublicDecoratorsAndVerifyContainer();
        }

        [Test]
        public void CommandValidationSucceedsWhenEndIsAfterStart()
        {
            var command = new StartAndEndDateCommand
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1)
            };

            var validator = this.container.GetInstance<StartAndEndDateCommandValidator>();
            var result = validator.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void CommandValidationFailsWhenEndIsBeforeStart()
        {
            var command = new StartAndEndDateCommand
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(-1)
            };

            var validator = this.container.GetInstance<StartAndEndDateCommandValidator>();
            var result = validator.Validate(command);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public async Task CommandExecutionSucceedsWhenValid()
        {
            var command = new StartAndEndDateCommand
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1)
            };

            await this.commandProcessor.ExecuteAsync(command);

            Assert.IsTrue(command.CommandExecuted);
        }

        [Test]
        public void CommandExecutionThrowsValidationExceptionWhenInvalid()
        {
            var command = new StartAndEndDateCommand
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(-1)
            };

            Assert.ThrowsAsync<ValidationException>(async () => await this.commandProcessor.ExecuteAsync(command));
        }

        [Test]
        public void QueryValidationSucceedsWhenEndIsAfterStart()
        {
            var query = new StartAndEndDate
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1)
            };

            var validator = this.container.GetInstance<StartAndEndDateValidator>();
            var result = validator.Validate(query);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void QueryValidationFailsWhenEndIsBeforeStart()
        {
            var command = new StartAndEndDate
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(-1)
            };

            var validator = this.container.GetInstance<StartAndEndDateValidator>();
            var result = validator.Validate(command);

            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public async Task QueryExecutionSucceedsWhenValid()
        {
            var query = new StartAndEndDate
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1)
            };

            var result = await this.queryProcessor.ExecuteAsync(query);

            Assert.IsTrue(result);
        }

        [Test]
        public void QueryExecutionThrowsValidationExceptionWhenInvalid()
        {
            var query = new StartAndEndDate
            {
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(-1)
            };

            Assert.ThrowsAsync<ValidationException>(async () => await this.queryProcessor.ExecuteAsync(query));
        }
    }
}
