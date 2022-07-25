using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;

internal class LongRunningQueryHandler : IQueryHandler<LongRunningQuery, int>
{
    public Task<int> HandleAsync(LongRunningQuery query)
    {
        return this.HandleAsync(query, default);
    }

    public async Task<int> HandleAsync(LongRunningQuery query, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        return 1;
    }
}
