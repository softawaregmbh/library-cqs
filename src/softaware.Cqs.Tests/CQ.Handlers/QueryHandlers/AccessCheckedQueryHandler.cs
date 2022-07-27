using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;

public class AccessCheckedQueryHandler : IRequestHandler<AccessCheckedQuery, bool>
{
    public Task<bool> HandleAsync(AccessCheckedQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
