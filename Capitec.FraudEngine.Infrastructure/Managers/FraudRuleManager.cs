using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Application.Abstractions.Caching;
using Capitec.FraudEngine.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Managers
{
    public class FraudRuleManager(IRuleRepository ruleRepository, IEnumerable<IFraudRule> rules, IRuleCacheInvalidator cacheInvalidator): IFraudRuleManager
    {
        public async Task InitializeRulesAsync(CancellationToken ct = default)
        {
            var activeConfigs = await ruleRepository.GetActiveRulesAsync(ct);

            foreach (var config in activeConfigs)
            {
                var ruleInstance = rules.FirstOrDefault(r =>
                    r.RuleKey.Equals(config.RuleName, StringComparison.OrdinalIgnoreCase));

                if (ruleInstance is VelocityRule velocityRule && !string.IsNullOrWhiteSpace(config.Parameters))
                {
                    velocityRule.UpdateParameters(config.Parameters);
                }


            }
        }


        public async Task SynchronizeAllRulesAsync(CancellationToken ct = default)
        {
            cacheInvalidator.Invalidate();

            var activeConfigs = await ruleRepository.GetActiveRulesAsync(ct);
            foreach (var config in activeConfigs)
            {
                var ruleInstance = rules.FirstOrDefault(r =>
                    r.RuleKey.Equals(config.RuleName, StringComparison.OrdinalIgnoreCase));

                if (ruleInstance is VelocityRule velocityRule)
                {
                    velocityRule.UpdateParameters(config.Parameters!);
                }
            }
        }
    }
}
