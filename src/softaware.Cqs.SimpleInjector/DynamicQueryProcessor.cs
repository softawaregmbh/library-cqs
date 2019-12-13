using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.SimpleInjector
{
    public class DynamicQueryProcessor : IQueryProcessor
    {
        private readonly Container container;

        public DynamicQueryProcessor(Container container)
        {
            this.container = container;
        }

        public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = this.container.GetInstance(handlerType);

            return handler.HandleAsync((dynamic)query, cancellationToken);
        }
    }
}
