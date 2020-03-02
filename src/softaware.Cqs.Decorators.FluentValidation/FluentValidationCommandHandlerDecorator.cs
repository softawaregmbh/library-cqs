using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace softaware.Cqs.Decorators.FluentValidation
{
    /// <summary>
    /// A decorator for validating the specified command with FluentValidation (https://fluentvalidation.net/). Uses the contructor injected <see cref="IValidator{T}"/> for validating the command.
    /// Throws a <see cref="ValidationException"/> when the validation fails.
    /// </summary>
    /// <typeparam name="TCommand">The command to execute.</typeparam>
    public class FluentValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator<TCommand> validator;
        private readonly ICommandHandler<TCommand> decoratee;

        /// <inheritdoc />
        public FluentValidationCommandHandlerDecorator(
            IValidator<TCommand> validator,
            ICommandHandler<TCommand> decoratee)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc />
        public Task HandleAsync(TCommand command) => this.HandleAsync(command, default);

        /// <inheritdoc />
        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await this.validator.ValidateAndThrowAsync(command, cancellationToken: cancellationToken);
            await this.decoratee.HandleAsync(command, cancellationToken);
        }
    }
}
