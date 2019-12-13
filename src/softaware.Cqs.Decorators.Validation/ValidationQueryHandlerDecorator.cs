#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.Validation
{
    /// <summary>
    /// A decorator for validating the specified query. Uses the contructor injected <see cref="IValidator"/> for validating the query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query to execute.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class ValidationQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IValidator validator;
        private readonly IQueryHandler<TQuery, TResult> decoratee;

        public ValidationQueryHandlerDecorator(
            IValidator validator,
            IQueryHandler<TQuery, TResult> decoratee)
        {
            this.validator = validator;
            this.decoratee = decoratee;
        }

        public Task<TResult> HandleAsync(TQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            this.validator.ValidateObject(query);
            return this.decoratee.HandleAsync(query);
        }
    }
}
