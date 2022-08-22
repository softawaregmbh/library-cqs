using NUnit.Framework;

namespace softaware.Cqs.Tests;

public abstract class TestBase
{
    protected IRequestProcessor requestProcessor;

    [SetUp]
    public virtual void SetUp()
    {
        this.requestProcessor = this.GetRequestProcessor();
    }

    protected abstract IRequestProcessor GetRequestProcessor();
}
