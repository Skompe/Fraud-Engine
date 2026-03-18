using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Repositories
{
    public class CustomerRepository(FraudDbContext dbContext) : ICustomerRepository
    {
        public async Task AddBatchAsync(IEnumerable<Customer> customers, CancellationToken ct)
        {
            var incomingIds = customers.Select(c => c.CustomerId).ToList();

            var existingIds = await dbContext.Customers
                .Where(c => incomingIds.Contains(c.CustomerId))
                .Select(c => c.CustomerId)
                .ToListAsync(ct);

            var newCustomers = customers.Where(c => !existingIds.Contains(c.CustomerId)).ToList();

            if (newCustomers.Any())
            {
                await dbContext.Customers.AddRangeAsync(newCustomers, ct);
                await dbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            var query = dbContext.Customers.AsNoTracking();

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
