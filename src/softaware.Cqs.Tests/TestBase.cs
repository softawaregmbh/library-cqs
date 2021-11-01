using NUnit.Framework;

namespace softaware.Cqs.Tests
{
    public abstract class TestBase
    {
        protected ICommandProcessor commandProcessor;
        protected IQueryProcessor queryProcessor;

        [SetUp]
        public virtual void SetUp()
        {
            this.commandProcessor = this.GetCommandProcessor();
            this.queryProcessor = this.GetQueryProcessor();
        }

        protected abstract ICommandProcessor GetCommandProcessor();
        protected abstract IQueryProcessor GetQueryProcessor();
    }
}
