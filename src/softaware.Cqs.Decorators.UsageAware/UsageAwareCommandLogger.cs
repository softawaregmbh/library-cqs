#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using softaware.UsageAware;
using System;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A <see cref="UsageAwareLogger{T}"/> for logging commands.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to log.</typeparam>
    public class UsageAwareCommandLogger<TCommand> : UsageAwareLogger<TCommand>
        where TCommand : ICommand
    {
        public UsageAwareCommandLogger(IUsageAwareLogger logger)
            : base(logger)
        {
        }

        public Task TimeAndLogCommandAsync(Func<Task> command)
        {
            return this.TimeAndLogAsync(command, LogType.Command);
        }
    }
}
