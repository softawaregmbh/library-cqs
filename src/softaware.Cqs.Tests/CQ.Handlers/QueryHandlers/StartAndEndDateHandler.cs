using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;

internal class StartAndEndDateHandler : IRequestHandler<StartAndEndDate, bool>
{
    public Task<bool> HandleAsync(StartAndEndDate query, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
