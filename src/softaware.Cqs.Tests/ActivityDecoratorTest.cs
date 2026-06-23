using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.OpenTelemetry;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

[TestFixture]
public abstract class ActivityDecoratorTest : TestBase
{
    protected abstract ActivityCollector GetActivityCollector();

    [Test]
    public void TestActivityForCommands()
    {
        var command = new SimpleCommand(1);

        this.requestProcessor.HandleAsync(command, default);

        var activity = this.GetActivityCollector().StoppedActivities.SingleOrDefault();
        Assert.That(activity, Is.Not.Null);
        Assert.That(activity!.OperationName, Is.EqualTo("SimpleCommand"));
        Assert.That(activity.Kind, Is.EqualTo(ActivityKind.Internal));
        Assert.That(activity.GetTagItem("cqs.request.type"), Is.EqualTo(typeof(SimpleCommand).FullName));
    }

    [Test]
    public void TestActivityForQueries()
    {
        var query = new GetSquare(4);

        this.requestProcessor.HandleAsync(query, default);

        var activity = this.GetActivityCollector().StoppedActivities.SingleOrDefault();
        Assert.That(activity, Is.Not.Null);
        Assert.That(activity!.OperationName, Is.EqualTo("GetSquare"));
        Assert.That(activity.Kind, Is.EqualTo(ActivityKind.Internal));
        Assert.That(activity.GetTagItem("cqs.request.type"), Is.EqualTo(typeof(GetSquare).FullName));
    }

    private class SimpleInjectorTest
        : ActivityDecoratorTest
    {
        private Container container;
        private ActivityCollector activityCollector;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.activityCollector = new ActivityCollector(CqsActivitySource.Name);

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(ActivityRequestHandlerDecorator<,>)));

            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        [TearDown]
        public void TearDown() => this.activityCollector.Dispose();

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
        protected override ActivityCollector GetActivityCollector() => this.activityCollector;
    }

    private class ServiceCollectionTest
        : ActivityDecoratorTest
    {
        private IServiceProvider serviceProvider;
        private ActivityCollector activityCollector;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            this.activityCollector = new ActivityCollector(CqsActivitySource.Name);

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b.AddOpenTelemetryActivityDecorator());

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        [TearDown]
        public void TearDown() => this.activityCollector.Dispose();

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();
        protected override ActivityCollector GetActivityCollector() => this.activityCollector;
    }
}
