using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;

internal class CallbackQueryHandler : IQueryHandler<CallbackQuery, int>
{
    public Task<int> HandleAsync(CallbackQuery query, CancellationToken cancellationToken)
    {
        query.Action();

        if (query.ShouldThrow)
        {
            throw new InvalidOperationException("We throw here for testing the rollback of transactions.");
        }

        return Task.FromResult(1);
    }
}
