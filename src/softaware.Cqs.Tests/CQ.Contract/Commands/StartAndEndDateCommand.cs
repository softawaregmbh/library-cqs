namespace softaware.Cqs.Tests.CQ.Contract.Commands;

public class StartAndEndDateCommand : ICommand
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public bool CommandExecuted { get; set; }
}
