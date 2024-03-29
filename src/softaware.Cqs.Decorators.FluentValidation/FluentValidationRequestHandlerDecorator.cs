#nullable enable

using FluentValidation;
using FluentValidation.Results;

namespace softaware.Cqs.Decorators.FluentValidation;

/// <summary>
/// A decorator for validating the specified <see cref="IRequest{TResult}"/>s with FluentValidation (https://fluentvalidation.net/).
/// Throws a <see cref="ValidationException"/> when the validation fails.
/// </summary>
/// <typeparam name="TRequest">The type of the request to execute.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class FluentValidationRequestHandlerDecorator<TRequest, TResult> : IRequestHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IReadOnlyList<IValidator<TRequest>> validators;
    private readonly IRequestHandler<TRequest, TResult> decoratee;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationRequestHandlerDecorator{TQuery, TResult}"/> class.
    /// </summary>
    /// <param name="validators">The list of validators to apply.</param>
    /// <param name="decoratee">The decorated query handler.</param>
    public FluentValidationRequestHandlerDecorator(
        IEnumerable<IValidator<TRequest>> validators,
        IRequestHandler<TRequest, TResult> decoratee)
    {
        this.validators = validators?.ToList() ?? throw new ArgumentNullException(nameof(this.validators));
        this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
    }

    /// <inheritdoc />
    public async Task<TResult> HandleAsync(TRequest query, CancellationToken cancellationToken)
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
