using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Application;
using MediatR;
using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capitec.FraudEngine.Application.Behaviors;
using Capitec.FraudEngine.Infrastructure.Persistence;

namespace Capitec.FraudEngine.Tests.Solution
{
    public class ArchitectureTests
    {
        private const string DomainNamespace = "Capitec.FraudEngine.Domain";
        private const string ApplicationNamespace = "Capitec.FraudEngine.Application";
        private const string InfrastructureNamespace = "Capitec.FraudEngine.Infrastructure";
        private const string ApiNamespace = "Capitec.FraudEngine.API";

      
        [Fact]
        public void DomainLayer_ShouldNot_HaveDependenciesOnOtherProjects()
        {
            var domainAssembly = typeof(Capitec.FraudEngine.Domain.Entities.Transaction).Assembly;

            var result = Types.InAssembly(domainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, "The Domain layer must be pure and have no external dependencies.");
        }

        [Fact]
        public void ApplicationLayer_ShouldNot_HaveDependenciesOnInfrastructureOrApi()
        {
            var applicationAssembly = typeof(ValidationBehavior<,>).Assembly;

            var result = Types.InAssembly(applicationAssembly)
                .ShouldNot()
                .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, "The Application layer must only depend on the Domain, never Infrastructure or API.");
        }

        [Fact]
        public void InfrastructureLayer_ShouldNot_HaveDependenciesOnApi()
        {
            var infrastructureAssembly = typeof(FraudDbContext).Assembly;

            var result = Types.InAssembly(infrastructureAssembly)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful, "Infrastructure cannot depend on the API presentation layer.");
        }

        [Fact]
        public void MediatR_CommandsAndQueries_Should_HaveCorrectSuffixes()
        {
            
            var commands = Types.InAssembly(typeof(ValidationBehavior<,>).Assembly)
                .That().ImplementInterface(typeof(IRequest<>))
                .And().DoNotHaveNameEndingWith("Query")
                .Should().HaveNameEndingWith("Command")
                .GetResult();

            Assert.True(commands.IsSuccessful, "Commands must end in 'Command'.");
        }

        [Fact]
        public void ApplicationLayer_Should_Not_ReferenceEntityFramework()
        {

            var appAssembly = typeof(ValidationBehavior<,>).Assembly;

            var result = Types.InAssembly(appAssembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            Assert.True(result.IsSuccessful, "Application layer must use IRepository abstractions, not EF Core directly.");
        }

    }
}
