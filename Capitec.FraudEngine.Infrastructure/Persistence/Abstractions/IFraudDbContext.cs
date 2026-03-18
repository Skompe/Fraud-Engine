using Capitec.FraudEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Persistence.Abstractions
{
    public interface IFraudDbContext
    {
        DbSet<Transaction> Transactions { get; }
        DbSet<FraudFlag> FraudFlags { get; }
        DbSet<RuleConfiguration> RuleConfigurations { get; }
        DbSet<User> Users { get; }
        DbSet<Customer> Customers { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
