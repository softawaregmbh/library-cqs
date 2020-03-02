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
    /// <typeparam name="TQuery">The type of the query to execute.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class FluentValidationQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IValidator<TQuery> validator;
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        /// <inheritdoc />
        public FluentValidationQueryHandlerDecorator(
            IValidator<TQuery> validator,
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc />
        public Task<TResult> HandleAsync(TQuery query) => this.HandleAsync(query, default);

        /// <inheritdoc />
        public async Task<TResult> HandleAsync(TQuery command, CancellationToken cancellationToken)
        {
            await this.validator.ValidateAndThrowAsync(command, cancellationToken: cancellationToken);
            return await this.decoratee.HandleAsync(command, cancellationToken);
        }
    }
}
