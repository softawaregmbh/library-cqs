namespace softaware.Cqs.Tests.CQ.Contract.Queries;

public class CallbackQuery : IQuery<int>
{
    public CallbackQuery(Action action, bool shouldThrow)
    {
        this.Action = action;
        this.ShouldThrow = shouldThrow;
    }

    public Action Action { get; }
    public bool ShouldThrow { get; }
}
