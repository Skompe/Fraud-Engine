using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Capitec.FraudEngine.API.Constants;
using Capitec.FraudEngine.Application.Constants;

namespace Capitec.FraudEngine.Tests.API.Endpoints
{
    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        public virtual Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, "TestAdmin"),
                new Claim(ClaimTypes.Role, IdentityConstants.Roles.Admin)
            }, SecurityConstants.TestAuth.Scheme));

            var ticket = new AuthenticationTicket(principal, SecurityConstants.TestAuth.Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public virtual Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        {
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }

    public class RulesApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RulesApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                });

                builder.ConfigureTestServices(services =>
                {

                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            }).CreateClient();


        }

        [Fact]
        public async Task GetActiveRules_ReturnsOk_WithData()
        {
            // Act
            var response = await _client.GetAsync("/api/rules");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetRuleByName_WithInvalidName_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/rules/RuleThatDoesNotExist123");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
