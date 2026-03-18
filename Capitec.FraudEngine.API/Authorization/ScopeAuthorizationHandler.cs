using Microsoft.AspNetCore.Authorization;
using Capitec.FraudEngine.API.Constants;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.API.Authorization
{
  
    public class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,ScopeRequirement requirement)
        {
            
            var scopeClaims = context.User
                .FindAll(SecurityConstants.Claims.Scope)
                .Select(c => c.Value)
                .ToList();

            
            if (scopeClaims.Contains(requirement.RequiredScope))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
