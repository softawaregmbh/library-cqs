#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace softaware.Cqs.Decorators.FluentValidation
{
    /// <summary>
    /// A decorator for validating the specified command with FluentValidation (https://fluentvalidation.net/).
    /// Throws a <see cref="ValidationException"/> when the validation fails.
    /// </summary>
    /// <typeparam name="TCommand">The command to execute.</typeparam>
    public class FluentValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IReadOnlyList<IValidator<TCommand>> validators;
        private readonly ICommandHandler<TCommand> decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationCommandHandlerDecorator{TCommand}"/> class.
        /// </summary>
        /// <param name="validators">The list of validators to apply.</param>
        /// <param name="decoratee">The decorated command handler.</param>
        public FluentValidationCommandHandlerDecorator(
            IEnumerable<IValidator<TCommand>> validators,
            ICommandHandler<TCommand> decoratee)
        {
            this.validators = validators?.ToList() ?? throw new ArgumentNullException(nameof(validators));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc />
        public Task HandleAsync(TCommand command) => this.HandleAsync(command, default);

        /// <inheritdoc />
        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            if (this.validators.Count == 1)
            {
                await this.validators[0].ValidateAndThrowAsync(command, cancellationToken: cancellationToken);
            }
            else if (this.validators.Count > 1)
            {
                var failures = new List<ValidationFailure>();
                foreach (var validator in this.validators)
                {
                    var validationResults = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);

                    if (!validationResults.IsValid)
                    {
                        failures.AddRange(validationResults.Errors);
                    }
                }

                if (failures.Count > 0)
                {
                    throw new ValidationException(failures);
                }
            }
            
            await this.decoratee.HandleAsync(command, cancellationToken);
        }
    }
}
