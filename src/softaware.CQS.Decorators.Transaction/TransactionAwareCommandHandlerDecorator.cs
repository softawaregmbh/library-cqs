using System;
using System.Threading.Tasks;
using System.Transactions;

namespace softaware.CQS.Decorators.Transaction
{
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
