#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.SimpleInjector
{
    /// <summary>
    /// Finds the matching <see cref="ICommandHandler{TCommand}"/> for a specified <see cref="ICommand"/> and
    /// calls <see cref="ICommandHandler{TCommand}.HandleAsync(TCommand, CancellationToken)"/> for that command handler.
    /// </summary>
    public class DynamicCommandProcessor : ICommandProcessor
    {
        private readonly Container container;

        public DynamicCommandProcessor(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// Finds the matching <see cref="ICommandHandler{TCommand}"/> for a specified <see cref="ICommand"/> and
        /// calls <see cref="ICommandHandler{TCommand}.HandleAsync(TCommand, CancellationToken)"/> for that command handler.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">The optional cancellation token when requesting the cancellation of the execution.</param>
        public Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            dynamic handler = this.container.GetInstance(handlerType);

            return handler.HandleAsync((dynamic)command, cancellationToken);
        }
    }
}
