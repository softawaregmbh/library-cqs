using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    /// <summary>
    /// Processes a specified command.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">The optional cancellation token for requesting cancellation of the command execution.</param>
        Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default);
    }
}
