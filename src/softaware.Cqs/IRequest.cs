namespace softaware.Cqs;

/// <summary>
/// Base interface for queries and commands.
/// </summary>
/// <typeparam name="TResult">The type of the request result.</typeparam>
/// <remarks>
/// This interface only exists as a common base type for <see cref="IQuery{TResult}"/> and <see cref="ICommand{TResult}"/>
/// and should not be implemented directly. </remarks>
public interface IRequest<TResult>
{
}
