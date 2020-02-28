using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    internal class StartAndEndDateCommandHandler : ICommandHandler<StartAndEndDateCommand>
    {
        public Task HandleAsync(StartAndEndDateCommand command)
        {
            // Just for testing: Change the value and assert the change in test afterwards.
            command.CommandExecuted = true;

            return Task.CompletedTask;
        }
    }
}
