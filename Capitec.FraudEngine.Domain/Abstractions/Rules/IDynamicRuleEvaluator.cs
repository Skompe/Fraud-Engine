using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Rules
{
    public interface IDynamicRuleEvaluator
    {
        Task<List<string>> EvaluateAsync(Transaction transaction, IEnumerable<RuleConfiguration> activeRules, CancellationToken ct = default);
        void ClearCache();
    }
}
