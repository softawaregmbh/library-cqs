using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests.CQ.Contract.Queries
{
    public class AccessCheckedQuery : IQuery<bool>, IAccessChecked
    {
        public bool AccessCheckHasBeenEvaluated { get; set; }
    }
}
