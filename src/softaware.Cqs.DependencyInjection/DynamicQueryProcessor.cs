using System;
using System.Threading;
using System.Threading.Tasks;
using softaware.Cqs;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DynamicQueryProcessor : IQueryProcessor
    {
        private readonly IServiceProvider serviceProvider;

        public DynamicQueryProcessor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            dynamic handler = this.serviceProvider.GetRequiredService(handlerType);

            return handler.HandleAsync(command, cancellationToken);
        }

        public async Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

            dynamic handler = this.serviceProvider.GetRequiredService(handlerType);

            return await handler.HandleAsync((dynamic)query, cancellationToken);
        }
    }
}
