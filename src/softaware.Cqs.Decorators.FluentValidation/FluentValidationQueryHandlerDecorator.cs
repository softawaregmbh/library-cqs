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
    /// <typeparam name="TQuery">The type of the query to execute.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class FluentValidationQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IReadOnlyList<IValidator<TQuery>> validators;
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationQueryHandlerDecorator{TQuery, TResult}"/> class.
        /// </summary>
        /// <param name="validators">The list of validators to apply.</param>
        /// <param name="decoratee">The decorated query handler.</param>
        public FluentValidationQueryHandlerDecorator(
            IEnumerable<IValidator<TQuery>> validators,
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.validators = validators?.ToList() ?? throw new ArgumentNullException(nameof(this.validators));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc />
        public Task<TResult> HandleAsync(TQuery query) => this.HandleAsync(query, default);

        /// <inheritdoc />
        public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
        {
            if (this.validators.Count == 1)
            {
                await this.validators[0].ValidateAndThrowAsync(query, cancellationToken: cancellationToken);
            }
            else if (this.validators.Count > 1)
            {
                var failures = new List<ValidationFailure>();
                foreach (var validator in this.validators)
                {
                    var validationResults = await validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);

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

            return await this.decoratee.HandleAsync(query, cancellationToken);
        }
    }
}
