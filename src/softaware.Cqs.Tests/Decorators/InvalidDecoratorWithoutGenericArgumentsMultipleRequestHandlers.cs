using softaware.Cqs.Tests.CQ.Contract.Commands;

namespace softaware.Cqs.Tests.Decorators;

public class InvalidDecoratorWithoutGenericArgumentsMultipleRequestHandlers
    : IRequestHandler<SimpleCommand, NoResult>,
    IRequestHandler<CommandWithResult, Guid>
{
    private readonly IRequestHandler<SimpleCommand, NoResult> simpleCommandDecoratee;
    private readonly IRequestHandler<CommandWithResult, Guid> commandWithResultDecoratee;

    public InvalidDecoratorWithoutGenericArgumentsMultipleRequestHandlers(
        IRequestHandler<SimpleCommand, NoResult> simpleCommandDecoratee,
        IRequestHandler<CommandWithResult, Guid> commandWithResultDecoratee)
    {
        this.simpleCommandDecoratee = simpleCommandDecoratee ?? throw new ArgumentNullException(nameof(simpleCommandDecoratee));
        this.commandWithResultDecoratee = commandWithResultDecoratee ?? throw new ArgumentNullException(nameof(commandWithResultDecoratee));
    }

    public async Task<NoResult> HandleAsync(SimpleCommand request, CancellationToken cancellationToken)
    {
        request.Value++;
        return await this.simpleCommandDecoratee.HandleAsync(request, cancellationToken);
    }

    public async Task<Guid> HandleAsync(CommandWithResult request, CancellationToken cancellationToken)
    {
        return await this.commandWithResultDecoratee.HandleAsync(request, cancellationToken);
    }
}
