using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Rules
{
    public class VelocityRule(ITransactionRepository transactionRepository) : IFraudRule
    {
        public string RuleKey => "HighVelocitySpend";

        private int _maxTransactions = 5;
        private int _timeWindowMinutes = 15;
        public async Task<string?> EvaluateAsync(Transaction transaction, CancellationToken ct = default)
        {
            var recentCount = await transactionRepository.GetTransactionCountAsync(transaction.CustomerId,TimeSpan.FromMinutes(_timeWindowMinutes),ct);

            if (recentCount >= _maxTransactions)
            {
                return RuleKey;
            }

            return null;
        }

        public void UpdateParameters(string jsonParameters)
        {
            if (string.IsNullOrWhiteSpace(jsonParameters)) return;

            using var parameters = JsonDocument.Parse(jsonParameters);
            if (parameters.RootElement.TryGetProperty("MaxTransactions", out var maxTx))
                _maxTransactions = maxTx.GetInt32();

            if (parameters.RootElement.TryGetProperty("TimeWindowMinutes", out var timeWin))
                _timeWindowMinutes = timeWin.GetInt32();
        }
    }
}
