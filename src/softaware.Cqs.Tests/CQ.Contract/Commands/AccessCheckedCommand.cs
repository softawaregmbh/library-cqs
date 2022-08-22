using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.CQ.Contract.Commands;

public class AccessCheckedCommand : ICommand, IAccessChecked
{
    public bool AccessCheckHasBeenEvaluated { get; set; }
}
