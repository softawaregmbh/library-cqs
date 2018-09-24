using softaware.UsageAware;
using System;
using System.Threading.Tasks;

namespace softaware.Cqs.Decorators.UsageAware
{
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
