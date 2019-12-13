using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);

        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            return this.HandleAsync(command);
        }
    }
}
