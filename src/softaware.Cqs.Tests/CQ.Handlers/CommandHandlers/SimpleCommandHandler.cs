using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    public class SimpleCommandHandler : ICommandHandler<SimpleCommand>
    {
        public Task HandleAsync(SimpleCommand command)
        {
            // Just for testing: Change the value and assert the change in test afterwards.
            command.Value += 1;
            return Task.CompletedTask;
        }
    }
}
