using System.Threading.Tasks;
using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers
{
    public class AccessCheckedCommandHandler : ICommandHandler<AccessCheckedCommand>
    {
        public Task HandleAsync(AccessCheckedCommand command)
        {
            // nothing to do.
            return Task.CompletedTask;
        }
    }
}
