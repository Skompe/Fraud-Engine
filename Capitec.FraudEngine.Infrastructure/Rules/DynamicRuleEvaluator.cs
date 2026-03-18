using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Rules
{
    public class DynamicRuleEvaluator(ILogger<DynamicRuleEvaluator> logger) : IDynamicRuleEvaluator
    {
        private static readonly ConcurrentDictionary<string, Func<Transaction, bool>> compiledRulesCache = new();

        public Task<List<string>> EvaluateAsync(Transaction transaction, IEnumerable<RuleConfiguration> activeRules, CancellationToken ct = default)
        {
            var triggeredRules = new List<string>();

            var dynamicRules = activeRules.Where(r => !string.IsNullOrWhiteSpace(r.Expression));

            foreach (var rule in dynamicRules)
            {
                if (!compiledRulesCache.TryGetValue(rule.Expression!, out var compiledFunc))
                {
                    try
                    {
                        var parsedExpression = DynamicExpressionParser.ParseLambda<Transaction, bool>(
                            new ParsingConfig(),
                            false,
                            rule.Expression!);

                        compiledFunc = parsedExpression.Compile();
                        compiledRulesCache.TryAdd(rule.Expression!, compiledFunc);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Compilation Failed: Dynamic rule '{RuleName}' has an invalid expression: {Expression}", rule.RuleName, rule.Expression);

                        continue;
                    }
                }

                if (compiledFunc == null)
                {
                    continue;
                }

                try
                {
                    if (compiledFunc(transaction))
                    {
                        triggeredRules.Add(rule.RuleName);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Evaluation Failed: Rule '{RuleName}' threw an error while processing Transaction {TransactionId}.", rule.RuleName, transaction.TransactionId);
                }
            }

            return Task.FromResult(triggeredRules);
        }

        public void ClearCache()
        {
            compiledRulesCache.Clear();
            logger.LogInformation("Dynamic Rule compiled expression cache has been cleared.");
        }
    }
}

