namespace softaware.Cqs.Tests.CQ.Contract.Commands;

public class CallbackCommand : ICommand
{
    public CallbackCommand(Action action, bool shouldThrow)
    {
        this.Action = action;
        this.ShouldThrow = shouldThrow;
    }

    public Action Action { get; }
    public bool ShouldThrow { get; }
}
