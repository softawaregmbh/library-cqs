namespace softaware.Cqs;

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <remarks>Consider using <see cref="ICommand"/> instead.
/// In general commands shouldn't return any results, but in rare cases
/// it's easier and requires less code if they do. Use sparingly.</remarks>
public interface ICommand<TResult> : IRequest<TResult>
{
}

/// <summary>
/// Marker interface for commands that don't return a result.
/// </summary>
public interface ICommand : ICommand<NoResult>
{
}
