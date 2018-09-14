using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace softaware.CQS.SimpleInjector
{
    public class DynamicCommandProcessor : ICommandProcessor
    {
        private readonly Container container;

        public DynamicCommandProcessor(Container container)
        {
            this.container = container;
        }

        public Task ExecuteAsync(ICommand command)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            dynamic handler = this.container.GetInstance(handlerType);

            return handler.HandleAsync((dynamic)command);
        }
    }
}
