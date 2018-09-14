using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.CQS
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}
