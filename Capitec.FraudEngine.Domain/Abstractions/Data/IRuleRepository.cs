using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface IRuleRepository
    {
        Task<List<RuleConfiguration>> GetActiveRulesAsync(CancellationToken ct);
        Task<RuleConfiguration?> GetByNameAsync(string name, CancellationToken ct);
        void Update(RuleConfiguration rule);
        Task<bool> ExistsAsync(string ruleName, CancellationToken ct);
        Task AddAsync(RuleConfiguration rule, CancellationToken ct);
        Task UpdateAsync(RuleConfiguration rule, CancellationToken cancellationToken = default);
    }
}
