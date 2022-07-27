using softaware.Cqs;

namespace ClassLibrary1
{
    public class Command : ICommand
    {
        public bool Decorated { get; set; }
    }

    public class Handler : IRequestHandler<Command, NoResult>
    {
        public Task<NoResult> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            return NoResult.Task;
        }
    }

    public class CommandHandlerDecorator : IRequestHandler<Command, NoResult>
    {
        private readonly IRequestHandler<Command, NoResult> inner;

        public CommandHandlerDecorator(IRequestHandler<Command, NoResult> inner)
        {
            this.inner = inner;
        }

        public async Task<NoResult> HandleAsync(Command request, CancellationToken cancellationToken)
        {
            request.Decorated = true;
            return await inner.HandleAsync(request, cancellationToken);
        }
    }
}
