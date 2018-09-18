using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.Validation
{
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
