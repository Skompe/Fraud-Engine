using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Repositories
{
    public class RuleRepository(IFraudDbContext context) : IRuleRepository
    {
        public async Task<List<RuleConfiguration>> GetActiveRulesAsync(CancellationToken ct)
        {
           return await context.RuleConfigurations.Where(r => r.IsActive).ToListAsync(ct);
        }

        public async Task<RuleConfiguration?> GetByNameAsync(string name, CancellationToken ct)
        {
            return await context.RuleConfigurations.FirstOrDefaultAsync(r => r.RuleName == name, ct);
        }

        public void Add(RuleConfiguration rule)
        {
            context.RuleConfigurations.Add(rule);
        }

        public void Update(RuleConfiguration rule)
        {
            context.RuleConfigurations.Update(rule);
        }

        public async Task<bool> ExistsAsync(string ruleName, CancellationToken ct)
        {
           return await context.RuleConfigurations.AnyAsync(r => r.RuleName == ruleName, ct);
        }

        public async Task AddAsync(RuleConfiguration rule, CancellationToken ct)
        {
            await context.RuleConfigurations.AddAsync(rule, ct).AsTask();
        }

        public async Task UpdateAsync(RuleConfiguration rule, CancellationToken cancellationToken = default)
        {
            context.RuleConfigurations.Update(rule);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
