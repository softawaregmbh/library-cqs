using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    public class CallbackCommandHandler : ICommandHandler<CallbackCommand>
    {
        public Task HandleAsync(CallbackCommand command)
        {
            command.Action();

            if (command.ShouldThrow)
            {
                throw new Exception("We throw here for testing the rollback of transactions.");
            }

            return Task.CompletedTask;
        }
    }
}
