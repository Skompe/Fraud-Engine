using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Application.Constants;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Repositories
{
    public class CachedRuleRepository(RuleRepository innerRepository, IMemoryCache memoryCache, IDynamicRuleEvaluator evaluator) : IRuleRepository
    {
        private const string ActiveRulesCacheKey = CacheKeys.Rules.ActiveRules;
        
        public async Task<List<RuleConfiguration>> GetActiveRulesAsync(CancellationToken ct)
        {
            if (memoryCache.TryGetValue(ActiveRulesCacheKey, out List<RuleConfiguration>? cachedRules))
            {
                return cachedRules ?? [];
            }

            var rulesFromDb = await innerRepository.GetActiveRulesAsync(ct);

            memoryCache.Set(
                ActiveRulesCacheKey,
                rulesFromDb,
                TimeSpan.FromMinutes(30));

            return rulesFromDb ?? [];
        }

        public async Task<RuleConfiguration?> GetByNameAsync(string ruleName, CancellationToken ct)
        {
           
            return await innerRepository.GetByNameAsync(ruleName, ct);
        }

  
        public void Add(RuleConfiguration rule)
        {
            
            innerRepository.Add(rule);

            ClearCache();
        }


        public void Update(RuleConfiguration rule)
        {
            innerRepository.Update(rule);

            ClearCache();
        }

        public async Task UpdateAsync(RuleConfiguration rule, CancellationToken cancellationToken = default)
        {
            await innerRepository.UpdateAsync(rule, cancellationToken);
            ClearCache();
        }

        public async Task<bool> ExistsAsync(string ruleName, CancellationToken ct)
        {
            
            return await innerRepository.ExistsAsync(ruleName, ct);
        }

        public async Task AddAsync(RuleConfiguration rule, CancellationToken ct)
        {
            
            await innerRepository.AddAsync(rule, ct);

            ClearCache();
        }

        public void ClearCache()
        {
            
            memoryCache.Remove(ActiveRulesCacheKey);

            evaluator.ClearCache();
        }
    }
}
