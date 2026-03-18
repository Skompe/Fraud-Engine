using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Persistence
{
    public class FraudDbContext : DbContext, IFraudDbContext, IUnitOfWork
    {
        public FraudDbContext(DbContextOptions<FraudDbContext> options) : base(options) { }

        public DbSet<FraudFlag> FraudFlags => Set<FraudFlag>();
        public DbSet<RuleConfiguration> RuleConfigurations => Set<RuleConfiguration>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Transaction>()
                .HasOne<Customer>()
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RuleConfiguration>()
                .HasIndex(r => r.RuleName)
                .IsUnique();


            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<Customer>()
            .HasKey(c => c.CustomerId);

            modelBuilder.Entity<Transaction>().HasIndex(t => new { t.CustomerId, t.Timestamp })
                .HasDatabaseName("Ix_Transactions_CustomerId_Timestamp");
        }
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => base.SaveChangesAsync(ct);
    }
}
