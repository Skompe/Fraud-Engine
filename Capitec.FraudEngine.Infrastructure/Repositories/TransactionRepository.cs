
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
    public class TransactionRepository(IFraudDbContext context) : ITransactionRepository
    {
        public async Task<Transaction?> GetByIdAsync(string id, CancellationToken ct)
        {
          return  await context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id, ct);
        }

        public async Task<List<Transaction>> GetByCustomerIdAsync(string customerId, CancellationToken ct)
        {
           return await context.Transactions.Where(t => t.CustomerId == customerId).ToListAsync(ct);
        }

        public async Task<bool> ExistsAsync(string transactionId, CancellationToken ct)
        {
           return await context.Transactions.AnyAsync(t => t.TransactionId == transactionId, ct);
        }

        public async Task AddAsync(Transaction transaction, CancellationToken ct)
        {
            await context.Transactions.AddAsync(transaction, ct).AsTask();
        }

        public async Task<int> GetTransactionCountAsync(string customerId, TimeSpan window, CancellationToken ct)
        {
            var cutoffTime = DateTime.UtcNow.Subtract(window);

           
            return await context.Transactions
                .AsNoTracking() 
                .Where(t => t.CustomerId == customerId && t.Timestamp >= cutoffTime)
                .CountAsync(ct);
        }
    }
}
