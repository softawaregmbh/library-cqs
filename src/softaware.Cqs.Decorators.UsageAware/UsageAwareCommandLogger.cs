#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Threading.Tasks;
using softaware.UsageAware;

namespace softaware.Cqs.Decorators.UsageAware
{
    /// <summary>
    /// A <see cref="UsageAwareLogger"/> for logging commands.
    /// </summary>
    public class UsageAwareCommandLogger : UsageAwareLogger
    {
        public UsageAwareCommandLogger(IUsageAwareLogger logger)
            : base(logger)
        {
        }

        public Task TimeAndLogCommandAsync<TCommand>(Func<Task> command)
            where TCommand : ICommand
        {
            return this.TimeAndLogAsync<TCommand>(command, LogType.Command);
        }
    }
}
