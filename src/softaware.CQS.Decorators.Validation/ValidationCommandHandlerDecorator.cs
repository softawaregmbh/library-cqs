using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace softaware.CQS.Decorators.Validation
{
    public class ValidationCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator validator;
        private readonly ICommandHandler<TCommand> decoratee;

        public ValidationCommandHandlerDecorator(
            IValidator validator,
            ICommandHandler<TCommand> decoratee)
        {
            this.validator = validator;
            this.decoratee = decoratee;
        }

        public Task HandleAsync(TCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            this.validator.ValidateObject(command);
            return this.decoratee.HandleAsync(command);
        }
    }
}
