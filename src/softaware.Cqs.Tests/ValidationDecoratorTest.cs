using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimpleInjector;
using softaware.Cqs.Decorators.Validation;
using softaware.Cqs.Tests.CQ.Contract.Commands;
using softaware.Cqs.Tests.CQ.Contract.Queries;
using softaware.Cqs.Tests.Fakes;

namespace softaware.Cqs.Tests;

[TestFixture]
public abstract class ValidationDecoratorTest : TestBase
{
    [Test]
    public async Task TestCommandValidation_RequiredPropertySet_Succeeds()
    {
        var command = new ValidationCommand
        {
            Value = "some value"
        };

        await this.requestProcessor.HandleAsync(command, default);
    }

    [Test]
    public void TestCommandValidation_RequiredPropertyNotSet_Throws()
    {
        var command = new ValidationCommand
        {
            Value = null
        };

        var exception = Assert.ThrowsAsync<ValidationException>(async () => await this.requestProcessor.HandleAsync(command, default));
        Assert.That(exception!.Message, Is.EqualTo("The Value field is required."));
    }

    [Test]
    public async Task TestQueryValidation_PropertyInRange_Succeeds()
    {
        var query = new GetSquare(4);

        var result = await this.requestProcessor.HandleAsync(query, default);

        Assert.That(result, Is.EqualTo(16));
    }

    [Test]
    public void TestQueryValidation_PropertyOutsideRange_Throws()
    {
        var query = new GetSquare(0);

        var exception = Assert.ThrowsAsync<ValidationException>(async () => await this.requestProcessor.HandleAsync(query, default));
        Assert.That(exception!.Message, Is.EqualTo("The field Value must be between 1 and 10."));
    }

    private class SimpleInjectorTest
        : ValidationDecoratorTest
    {
        private Container container;

        [SetUp]
        public override void SetUp()
        {
            this.container = new Container();

            this.container
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b
                    .AddRequestHandlerDecorator(typeof(ValidationRequestHandlerDecorator<,>)));

            this.container.RegisterInstance<IValidator>(new DataAnnotationsValidator());
            this.container.Register<IDependency, Dependency>();

            this.container.Verify();

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.container.GetRequiredService<IRequestProcessor>();
    }

    private class ServiceCollectionTest
        : ValidationDecoratorTest
    {
        private IServiceProvider serviceProvider;

        [SetUp]
        public override void SetUp()
        {
            var services = new ServiceCollection();

            services
                .AddSoftawareCqs(b => b.IncludeTypesFrom(Assembly.GetExecutingAssembly()))
                .AddDecorators(b => b.AddDataAnnotationsValidationDecorators());

            services.AddTransient<IDependency, Dependency>();

            this.serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            base.SetUp();
        }

        protected override IRequestProcessor GetRequestProcessor() => this.serviceProvider.GetRequiredService<IRequestProcessor>();
    }
}
