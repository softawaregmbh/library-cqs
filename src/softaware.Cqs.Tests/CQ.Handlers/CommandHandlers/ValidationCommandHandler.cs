using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    public class ValidationCommandHandler : ICommandHandler<ValidationCommand>
    {
        public Task HandleAsync(ValidationCommand command)
        {
            return Task.CompletedTask;
        }
    }
}
