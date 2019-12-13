using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.SimpleInjector
{
    public class DynamicCommandProcessor : ICommandProcessor
    {
        private readonly Container container;

        public DynamicCommandProcessor(Container container)
        {
            this.container = container;
        }

        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            dynamic handler = this.container.GetInstance(handlerType);

            return handler.HandleAsync((dynamic)command, cancellationToken);
        }
    }
}
