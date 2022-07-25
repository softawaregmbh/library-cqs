using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;

internal class GetSquareHandler : IQueryHandler<GetSquare, int>
{
    public Task<int> HandleAsync(GetSquare query, CancellationToken cancellationToken)
    {
        return Task.FromResult(query.Value * query.Value);
    }
}
