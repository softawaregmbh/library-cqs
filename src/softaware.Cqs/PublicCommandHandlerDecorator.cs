using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// This decorator just delegates to the next decorator or to the actual handler.
    /// </summary>
    /// <remarks>
    /// This is needed since the introduction of the default interface implementation for the <see cref="ICommandHandler{TCommand}.HandleAsync(TCommand, CancellationToken)"/> overload,
    /// so that this overload can corretly be resolved by the DI container. See https://jeremybytes.blogspot.com/2019/09/c-8-interfaces-dynamic-and-default.html for details.
    /// </remarks>
    public class PublicCommandHandlerDecorator<TCommand>
        : ICommandHandler<TCommand> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicCommandHandlerDecorator{TCommand}"/> class.
        /// </summary>
        /// <param name="decoratee">The command handler to decorate.</param>
        public PublicCommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        /// <inheritdoc/>
        public Task HandleAsync(TCommand command)
        {
            return this.HandleAsync(command, default);
        }

        /// <inheritdoc/>
        public Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            return this.decoratee.HandleAsync(command, cancellationToken);
        }
    }
}
