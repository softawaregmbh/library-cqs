#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A decorator for tracking command executions with UsageAware.
    /// </summary>
    /// <typeparam name="TCommand">The command to execute.</typeparam>
    public class UsageAwareCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly UsageAwareCommandLogger logger;
        private readonly ICommandHandler<TCommand> decoratee;

        public UsageAwareCommandHandlerDecorator(
            UsageAwareCommandLogger logger,
            ICommandHandler<TCommand> decoratee)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task HandleAsync(TCommand command) => this.HandleAsync(command, default);

        public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            await this.logger.TimeAndLogCommandAsync<TCommand>(() => this.decoratee.HandleAsync(command, cancellationToken));
        }
    }
}
