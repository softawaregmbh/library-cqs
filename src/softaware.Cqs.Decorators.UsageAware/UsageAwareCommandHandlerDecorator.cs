using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    public class UsageAwareCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly UsageAwareCommandLogger<TCommand> logger;
        private readonly ICommandHandler<TCommand> decoratee;

        public UsageAwareCommandHandlerDecorator(
            UsageAwareCommandLogger<TCommand> logger,
            ICommandHandler<TCommand> decoratee)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(TCommand command)
        {
            await this.logger.TimeAndLogCommandAsync(() => this.decoratee.HandleAsync(command));
        }
    }
}
