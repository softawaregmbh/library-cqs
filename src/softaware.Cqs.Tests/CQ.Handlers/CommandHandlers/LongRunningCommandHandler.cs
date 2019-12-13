using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    internal class LongRunningCommandHandler : ICommandHandler<LongRunningCommand>
    {
        public Task HandleAsync(LongRunningCommand command)
        {
            return this.HandleAsync(command, default);
        }

        public async Task HandleAsync(LongRunningCommand command, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }
}
