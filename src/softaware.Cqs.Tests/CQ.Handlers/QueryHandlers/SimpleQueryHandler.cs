using softaware.Cqs.Tests.CQ.Contract.Queries;

namespace softaware.Cqs.Tests.CQ.Handlers.QueryHandlers;
internal class SimpleQueryHandler
    : IRequestHandler<SimpleQuery1, string>,
    IRequestHandler<SimpleQuery2, string>
{
    public Task<string> HandleAsync(SimpleQuery1 request, CancellationToken cancellationToken) => Task.FromResult("Simple Query 1 Result");
    public Task<string> HandleAsync(SimpleQuery2 request, CancellationToken cancellationToken) => Task.FromResult("Simple Query 2 Result");
}
