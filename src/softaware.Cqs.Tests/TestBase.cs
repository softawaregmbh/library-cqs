using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.SimpleInjector;

namespace softaware.Cqs.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        protected Container container;
        protected ICommandProcessor commandProcessor;
        protected IQueryProcessor queryProcessor;

        protected void SetupContainer()
        {
            this.container = new Container();

            Assembly[] handlerAssemblies = new[] { Assembly.GetExecutingAssembly() };
            this.container.Register(typeof(ICommandHandler<>), handlerAssemblies);
            this.container.Register(typeof(IQueryHandler<,>), handlerAssemblies);

            this.commandProcessor = new DynamicCommandProcessor(this.container);
            this.queryProcessor = new DynamicQueryProcessor(this.container);
        }

        [SetUp]
        public virtual void SetUp()
        {
            this.SetupContainer();
        }
    }
}
