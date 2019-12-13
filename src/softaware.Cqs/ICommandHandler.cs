using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// The command handler which handles the specified command.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to handle.</typeparam>
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Handles a command.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        Task HandleAsync(TCommand command);

        /// <summary>
        /// Handles a command which cancellation support.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token when requesting the cancellation of the execution.</param>
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            return this.HandleAsync(command);
        }
    }
}
