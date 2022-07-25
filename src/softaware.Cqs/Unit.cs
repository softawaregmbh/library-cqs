namespace softaware.Cqs;

/// <summary>
/// Artificial type necessary because <see cref="void"/> cannot be used as a type return type.
/// </summary>
public record struct Void
{
    private static readonly Void OnlyValue;

    /// <summary>
    /// A value representing no result (void).
    /// </summary>
    public static ref readonly Void Value => ref OnlyValue;

    /// <summary>
    /// A Task returning <see cref="Value"/>.
    /// </summary>
    public static Task<Void> Task { get; } = System.Threading.Tasks.Task.FromResult(OnlyValue);
}
