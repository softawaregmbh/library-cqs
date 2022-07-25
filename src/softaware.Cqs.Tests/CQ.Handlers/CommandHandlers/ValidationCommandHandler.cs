using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.CQ.Handlers.CommandHandlers;

public class ValidationCommandHandler : ICommandHandler<ValidationCommand>
{
    public Task HandleAsync(ValidationCommand command, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
