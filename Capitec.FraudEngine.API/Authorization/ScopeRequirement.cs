using Microsoft.AspNetCore.Authorization;

namespace Capitec.FraudEngine.API.Authorization
{
    public class ScopeRequirement : IAuthorizationRequirement
    {
        public string RequiredScope { get; }

        public ScopeRequirement(string requiredScope)
        {
            RequiredScope = requiredScope;
        }
    }
}
