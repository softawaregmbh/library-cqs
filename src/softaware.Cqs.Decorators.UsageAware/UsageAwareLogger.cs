using softaware.UsageAware;
using System.Diagnostics;

namespace softaware.Cqs.Decorators.UsageAware;

/// <summary>
/// Logs and measures duration of the execution of a command or a query.
/// </summary>
/// <typeparam name="TRequest">The type of the request to log.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class UsageAwareLogger<TRequest, TResult>
{
    private enum LogType
    {
        Command,
        Query
    }

    private static readonly string Area;
    private static readonly string Action;
    private static readonly LogType Type;

    private readonly IUsageAwareLogger logger;

    static UsageAwareLogger()
    {
        var type = typeof(TRequest);
        Area = type.Namespace![(type.Namespace!.LastIndexOf('.') + 1)..];
        Action = type.Name;
        Type = type.GetInterface("ICommand`1")?.Namespace == "softaware.Cqs" ? LogType.Command : LogType.Query;
    }

    public UsageAwareLogger(IUsageAwareLogger logger) =>
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Task> TimeAndLogAsync(Func<Task> execute)
    {
        var watch = Stopwatch.StartNew();

        var task = execute();
        await task;

        var additionalProperties = new Dictionary<string, string>()
        {
            { "duration", watch.Elapsed.ToString() },
            { "type", Type.ToString() }
        };

        await this.logger.TrackActionAsync(Area, Action, additionalProperties);

        return task;
    }
}
