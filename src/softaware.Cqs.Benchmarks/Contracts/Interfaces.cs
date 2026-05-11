namespace softaware.Cqs.Benchmarks.Contracts;

/// <summary>
/// Marker interface for access-checked requests (command or query).
/// </summary>
public interface IAccessChecked
{
    bool AccessCheckEvaluated { get; set; }
}

/// <summary>
/// Marker interface for prioritized requests.
/// </summary>
public interface IPrioritized
{
    int Priority { get; set; }
}
