using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(string id, CancellationToken ct);
        Task<List<Transaction>> GetByCustomerIdAsync(string customerId, CancellationToken ct);
        Task<bool> ExistsAsync(string transactionId, CancellationToken ct);
        Task AddAsync(Transaction transaction, CancellationToken ct);
        Task<int> GetTransactionCountAsync(string customerId, TimeSpan window, CancellationToken ct);
    }
}
