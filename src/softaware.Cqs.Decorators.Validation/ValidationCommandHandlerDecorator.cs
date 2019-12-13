#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.Validation
{
    /// <summary>
    /// A decorator for validating the specified command. Uses the contructor injected <see cref="IValidator"/> for validating the command.
    /// </summary>
    /// <typeparam name="TCommand">The command to execute.</typeparam>
    public class ValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator validator;
        private readonly ICommandHandler<TCommand> decoratee;

        public ValidationCommandHandlerDecorator(
            IValidator validator,
            ICommandHandler<TCommand> decoratee)
        {
            this.validator = validator;
            this.decoratee = decoratee;
        }

        public Task HandleAsync(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            this.validator.ValidateObject(command);
            return this.decoratee.HandleAsync(command);
        }
    }
}
