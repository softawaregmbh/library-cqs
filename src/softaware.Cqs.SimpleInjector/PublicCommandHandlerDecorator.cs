#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace softaware.Cqs.SimpleInjector
{
    /// <summary>
    /// This decorator just delegates to the next decorator or to the actual handler.
    /// It is needed when using <see cref="DynamicCommandProcessor"/> to allow <see langword="internal"/> decorators or <see langword="internal"/> handlers to get resolved correctly.
    /// A <see langword="public"/> decorator must be applied first so that the <see cref="DynamicCommandProcessor"/> can call the correct method.
    /// <para>
    /// See https://github.com/dotnetjunkie/solidservices/issues/21#issuecomment-382506019 for more information.
    /// </para>
    /// <para>
    /// Register this decorator as the *last* decorator in the chain using <see cref="global::SimpleInjector.Container.RegisterDecorator(Type, Type)"/>.
    /// (The last decorator in the chain will be resolved *first*).
    /// </para>
    /// </summary>
    public class PublicCommandHandlerDecorator<TCommand>
        : ICommandHandler<TCommand> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> decoratee;

        public PublicCommandHandlerDecorator(ICommandHandler<TCommand> decoratee)
        {
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        public Task HandleAsync(TCommand command)
        {
            return this.HandleAsync(command, default);
        }

        public Task HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            return this.decoratee.HandleAsync(command, cancellationToken);
        }
    }
}
