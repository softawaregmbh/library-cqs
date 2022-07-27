namespace softaware.Cqs;

/// <summary>
/// Artificial type necessary because <see cref="void"/> cannot be used as return type.
/// </summary>
/// 
public record struct NoResult
{
    private static readonly NoResult Instance;

    /// <summary>
    /// A Task returning <see cref="NoResult"/>.
    /// </summary>
    public static Task<NoResult> Task { get; } = System.Threading.Tasks.Task.FromResult(Instance);

    /// <summary>
    /// A value representing no result (void).
    /// </summary>
    public static ref readonly NoResult Value => ref Instance;
}

