using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Constants;
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
    public class FraudFlagRepository(IFraudDbContext context) : IFraudFlagRepository
    {
        public async Task<FraudFlag?> GetByIdAsync(Guid id, CancellationToken ct)
        {
           return await context.FraudFlags.FindAsync([id], ct);
        }

        public async Task<List<FraudFlag>> GetPendingFlagsAsync(CancellationToken ct)
        {
            return await context.FraudFlags.Where(f => f.Status == DomainConstants.FraudStatus.Pending).ToListAsync(ct);
        }

        public async Task<List<FraudFlag>> GetByCustomerIdAsync(string customerId, CancellationToken ct)
        {
           return await context.FraudFlags.Where(f => f.CustomerId == customerId).ToListAsync(ct);
        }

        public void Add(FraudFlag flag)
        {
             context.FraudFlags.Add(flag);
        }
    }
}
