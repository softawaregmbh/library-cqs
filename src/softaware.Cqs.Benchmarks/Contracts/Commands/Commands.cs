namespace softaware.Cqs.Benchmarks.Contracts.Commands;

public class SimpleCommand : ICommand
{
    public int Value { get; set; }
}

public class AccessCheckedCommand : ICommand, IAccessChecked
{
    public bool AccessCheckEvaluated { get; set; }
}

public class HighPriorityCommand : ICommand, IPrioritized
{
    public int Priority { get; set; } = 1;
}

public class CommandWithResult : ICommand<int>
{
    public int Input { get; set; }
}
