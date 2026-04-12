namespace softaware.Cqs.Benchmarks.Contracts.Queries;

public class GetSquare : IQuery<int>
{
    public int Value { get; set; }
}

public class GetGreeting : IQuery<string>
{
    public string Name { get; set; } = "";
}

public class AccessCheckedQuery : IQuery<bool>, IAccessChecked
{
    public bool AccessCheckEvaluated { get; set; }
}
