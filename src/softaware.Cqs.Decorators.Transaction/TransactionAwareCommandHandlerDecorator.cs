#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Threading.Tasks;
using System.Transactions;

namespace softaware.Cqs.Decorators.Transaction
{
    /// <summary>
    /// A decorator for creating a <see cref="TransactionScope"/> for the command handler.
    /// The transaction gets committed when the decorated handler successfully executes.
    /// </summary>
    /// <typeparam name="TCommand">The command to execute.</typeparam>
    public class TransactionAwareCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> decoratee;

        public TransactionAwareCommandHandlerDecorator(
            ICommandHandler<TCommand> decoratee)
        {
            this.decoratee = decoratee;
        }

        public async Task HandleAsync(TCommand command)
        {
            TransactionOptions transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };

            using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                await this.decoratee.HandleAsync(command);
                tx.Complete();
            }
        }
    }
}
