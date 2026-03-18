using Capitec.FraudEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Domain.Abstractions.Data
{
    public interface ICustomerRepository
    {
        Task AddBatchAsync(IEnumerable<Customer> customers, CancellationToken ct);
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    }
}
