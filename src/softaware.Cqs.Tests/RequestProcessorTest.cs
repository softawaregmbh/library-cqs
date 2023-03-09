using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

public abstract class RequestProcessorTest : TestBase
{
    [Test]
    public async Task ExecuteCommand()
    {
        var command = new SimpleCommand(value: 1);

        await this.requestProcessor.HandleAsync(command, default);

        Assert.That(command.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteQuery()
    {
        var query = new GetSquare(value: 4);

        var result = await this.requestProcessor.HandleAsync(query, default);

        Assert.That(result, Is.EqualTo(16));
    }

    [Test]
    public async Task ExecuteSimpleQueriesInSameHandler()
    {
        var query1 = new SimpleQuery1();
        var query2 = new SimpleQuery2();

        var result1 = await this.requestProcessor.HandleAsync(query1, default);
        var result2 = await this.requestProcessor.HandleAsync(query2, default);

        Assert.That(result1, Is.EqualTo("Simple Query 1 Result"));
        Assert.That(result2, Is.EqualTo("Simple Query 2 Result"));
    }

    [Test]
    public abstract Task ExecuteCommandWithDependency();

    private abstract class SimpleInjectorTest
        : RequestProcessorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.RegisterDependency(this.container);

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()));

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
        protected abstract void RegisterDependency(Container container);

        [Test]
        public override async Task ExecuteCommandWithDependency()
        {
            using (Scope scope = AsyncScopedLifestyle.BeginScope(this.container))
            {
                var command = new CommandWithDependency();
                await this.requestProcessor.HandleAsync(command, default);
            }
        }

        private class TransientSimpleInjectorTest : SimpleInjectorTest
        {
            protected override void RegisterDependency(Container container)
            {
                container.Register<IDependency, Dependency>();
            }
        }

        private class ScopedSimpleInjectorTest : SimpleInjectorTest
        {
            protected override void RegisterDependency(Container container)
            {
                container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

                container.Register<IDependency, Dependency>(Lifestyle.Scoped);
            }
        }

        private class SingletonSimpleInjectorTest : SimpleInjectorTest
        {
            protected override void RegisterDependency(Container container)
            {
                container.RegisterSingleton<IDependency, Dependency>();
            }
        }
    }

    private abstract class ServiceCollectionTest
        : RequestProcessorTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()));

            this.RegisterDependency(services);

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();
        protected abstract void RegisterDependency(ServiceCollection serviceCollection);

        [Test]
        public override async Task ExecuteCommandWithDependency()
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                var requestProcessor = scope.ServiceProvider.GetRequiredService<IRequestProcessor>();

                var command = new CommandWithDependency();
                await requestProcessor.HandleAsync(command, default);
            }
        }

        private class TransientServiceCollectionTest : ServiceCollectionTest
        {
            protected override void RegisterDependency(ServiceCollection services)
            {
                services.AddTransient<IDependency, Dependency>();
            }
        }

        private class ScopedServiceCollectionTest : ServiceCollectionTest
        {
            protected override void RegisterDependency(ServiceCollection services)
            {
                services.AddScoped<IDependency, Dependency>();
            }
        }

        private class SingletonServiceCollectionTest : ServiceCollectionTest
        {
            protected override void RegisterDependency(ServiceCollection services)
            {
                services.AddSingleton<IDependency, Dependency>();
            }
        }
    }
}
