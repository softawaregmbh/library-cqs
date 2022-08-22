namespace softaware.Cqs.Tests.CQ.Contract.Commands;

public class SimpleCommand : ICommand
{
    public SimpleCommand(int value)
    {
        this.Value = value;
    }

    public int Value { get; set; }
}
