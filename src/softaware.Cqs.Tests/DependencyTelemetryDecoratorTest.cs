using System.Reflection;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.ApplicationInsights;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

[TestFixture]
public abstract class DependencyTelemetryDecoratorTest : TestBase
{
    protected abstract MockTelemetryChannel GetTelementryChannel();

    [Test]
    public void TestDependencyTelementryForCommands()
    {
        var command = new SimpleCommand(1);

        this.requestProcessor.HandleAsync(command, default);

        var telemetry = this.GetTelementryChannel().SentTelemetries.SingleOrDefault() as DependencyTelemetry;
        Assert.That(telemetry, Is.Not.Null);
        Assert.That(telemetry!.Type, Is.EqualTo("CQS"));
        Assert.That(telemetry.Name, Is.EqualTo("SimpleCommand"));
    }

    [Test]
    public void TestDependencyTelementryForQueries()
    {
        var query = new GetSquare(4);

        this.requestProcessor.HandleAsync(query, default);

        var telemetry = this.GetTelementryChannel().SentTelemetries.SingleOrDefault() as DependencyTelemetry;
        Assert.That(telemetry, Is.Not.Null);
        Assert.That(telemetry!.Type, Is.EqualTo("CQS"));
        Assert.That(telemetry.Name, Is.EqualTo("GetSquare"));
    }

    private class SimpleInjectorTest
        : DependencyTelemetryDecoratorTest
    {
        private Container container;
        private MockTelemetryChannel mockTelemetryChannel;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            var (client, channel) = FakeTelemetryClient.Create();

            this.container.RegisterInstance(client);

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(DependencyTelemetryRequestHandlerDecorator<,>)));

            this.mockTelemetryChannel = channel;

            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
        protected override MockTelemetryChannel GetTelementryChannel() => this.mockTelemetryChannel;
    }

    private class ServiceCollectionTest
        : DependencyTelemetryDecoratorTest
    {
        private IServiceProvider serviceProvider;
        private MockTelemetryChannel mockTelemetryChannel;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            var (client, channel) = FakeTelemetryClient.Create();

            services.AddSingleton(client);

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b.AddApplicationInsightsDependencyTelemetryDecorator());

            this.mockTelemetryChannel = channel;

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();
        protected override MockTelemetryChannel GetTelementryChannel() => this.mockTelemetryChannel;
    }
}
