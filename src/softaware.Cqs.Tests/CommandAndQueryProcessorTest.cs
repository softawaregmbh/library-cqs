using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests
{
    public abstract class CommandAndQueryProcessorTest : TestBase
    {
        [Test]
        public async Task ExecuteCommand()
        {
            var command = new SimpleCommand(value: 1);

            await this.commandProcessor.ExecuteAsync(command);

            Assert.That(command.Value, Is.EqualTo(2));
        }

        [Test]
        public async Task ExecuteQuery()
        {
            var query = new GetSquare(value: 4);

            var result = await this.queryProcessor.ExecuteAsync(query);

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
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => { });

                this.container.Verify();

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.container.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.container.GetRequiredService<IQueryProcessor>();
            protected abstract void RegisterDependency(Container container);

            [Test]
            public override async Task ExecuteCommandWithDependency()
            {
                using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
                {
                    var command = new CommandWithDependency();
                    await this.commandProcessor.ExecuteAsync(command);
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
                    .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                    .AddDecorators(b => { });

                this.RegisterDependency(services);

                this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

                base.SetUp();
            }

            protected override ICommandProcessor GetCommandProcessor() => this.serviceProvider.GetRequiredService<ICommandProcessor>();
            protected override IQueryProcessor GetQueryProcessor() => this.serviceProvider.GetRequiredService<IQueryProcessor>();
            protected abstract void RegisterDependency(ServiceCollection serviceCollection);

            [Test]
            public override async Task ExecuteCommandWithDependency()
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();

                    var command = new CommandWithDependency();
                    await commandProcessor.ExecuteAsync(command);
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
}