using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

public abstract class CommandAndQueryProcessorTest : TestBase
{
    [Test]
    public async Task ExecuteCommand()
    {
        var command = new SimpleCommand(value: 1);

        await this.requestProcessor.ExecuteAsync(command, default);

        Assert.That(command.Value, Is.EqualTo(2));
    }

    [Test]
    public async Task ExecuteQuery()
    {
        var query = new GetSquare(value: 4);

        var result = await this.requestProcessor.ExecuteAsync(query, default);

        Assert.That(result, Is.EqualTo(16));
    }

    [Test]
    public abstract Task ExecuteCommandWithDependency();

    private abstract class SimpleInjectorTest
        : CommandAndQueryProcessorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.RegisterDependency(container);

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
            using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
            {
                var command = new CommandWithDependency();
                await this.requestProcessor.ExecuteAsync(command, default);
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
        : CommandAndQueryProcessorTest
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
                await requestProcessor.ExecuteAsync(command, default);
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
