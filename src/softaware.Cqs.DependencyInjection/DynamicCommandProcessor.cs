using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace softaware.Cqs.DependencyInjection
{
    public class DynamicCommandProcessor : ICommandProcessor
    {
        private readonly IServiceProvider serviceProvider;

        public DynamicCommandProcessor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            dynamic handler = this.serviceProvider.GetRequiredService(handlerType);

            return handler.HandleAsync((dynamic)command, cancellationToken);
        }
    }
}
