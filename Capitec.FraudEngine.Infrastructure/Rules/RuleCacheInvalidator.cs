using Capitec.FraudEngine.Application.Abstractions.Caching;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Microsoft.Extensions.Caching.Memory;

namespace Capitec.FraudEngine.Infrastructure.Rules
{
    public class RuleCacheInvalidator(IMemoryCache memoryCache, IDynamicRuleEvaluator dynamicRuleEvaluator) : IRuleCacheInvalidator
    {
        public void Invalidate()
        {
            memoryCache.Remove(CacheKeys.Rules.ActiveRules);
            dynamicRuleEvaluator.ClearCache();
        }
    }
}
