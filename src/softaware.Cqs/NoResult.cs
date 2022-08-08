namespace softaware.Cqs;

/// <summary>
/// Artificial type necessary because <see cref="void"/> cannot be used as return type.
/// </summary>
/// 
public readonly record struct NoResult
{
    private static readonly NoResult Instance;

    /// <summary>
    /// A Task returning <see cref="NoResult"/>.
    /// </summary>
    public static Task<NoResult> CompletedTask { get; } = Task.FromResult(Instance);

    /// <summary>
    /// A value representing no result (void).
    /// </summary>
    public static ref readonly NoResult Value => ref Instance;
}

