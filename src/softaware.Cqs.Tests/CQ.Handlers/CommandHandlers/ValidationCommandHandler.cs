using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class ValidationCommandHandler : IRequestHandler<ValidationCommand, NoResult>
{
    public Task<NoResult> HandleAsync(ValidationCommand command, CancellationToken cancellationToken)
    {
        return NoResult.Task;
    }
}
