using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Application.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Persistence
{
    public class FraudDbSeeder(FraudDbContext context, ILogger<FraudDbSeeder> logger)
    {
      
        public async Task SeedAsync(bool isProduction)
        {
            try
            {
                if (!isProduction && context.Database.IsRelational())
                {
                    logger.LogInformation("Applying pending migrations...");
                    await context.Database.MigrateAsync();
                }

                if (isProduction)
                {
                    logger.LogInformation("Production environment detected. Skipping test data seeding.");
                    return;
                }

                logger.LogInformation("Development environment detected. Seeding database...");

               
                await SeedSystemAdminAsync();

                
                await SeedRulesAsync();

               
                await SeedCustomerVelocityScenarioAsync();

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                throw;
            }
        }

        private async Task SeedSystemAdminAsync()
        {
            if (await context.Users.AnyAsync(u => u.Username == "admin@capitec.co.za"))
                return;

            logger.LogInformation("Seeding Admin User...");
            var hasher = new PasswordHasher<User>();
            var adminUser = new User("admin@capitec.co.za", IdentityConstants.Roles.Admin, "temp-hash");
            var finalHash = hasher.HashPassword(adminUser, "Capitec@2026!");
            var finalAdminUser = new User("admin@capitec.co.za", IdentityConstants.Roles.Admin, finalHash);

            context.Users.Add(finalAdminUser);
            await context.SaveChangesAsync();
        }

        private async Task SeedRulesAsync()
        {
            if (await context.RuleConfigurations.AnyAsync()) return;

            logger.LogInformation("Seeding initial Dynamic Fraud Rules...");

            var initialRules = new List<RuleConfiguration>
            {
                new RuleConfiguration("HighValueTransaction", "Flags any transaction exceeding R50,000.", "Amount > 50000"),
                new RuleConfiguration("MicroTransactionTesting", "Flags extremely small transactions typically used to test stolen cards.", "Amount <= 5.00"),
                new RuleConfiguration("ForeignCurrencyHighRisk", "Flags high-value foreign currency transactions.", "Currency != \"ZAR\" AND Amount > 10000"),
                new RuleConfiguration("HighVelocitySpend", "Built-in Rule: Flags customers making too many transactions in a short window.", null, "{\"MaxTransactions\": 5, \"TimeWindowMinutes\": 15}")
            };

            await context.RuleConfigurations.AddRangeAsync(initialRules);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully seeded {Count} dynamic fraud rules.", initialRules.Count);
        }

        private async Task SeedCustomerVelocityScenarioAsync()
        {
            var testCustomerId = "CUST_8899_001";

            if (!await context.Customers.AnyAsync(c => c.CustomerId == testCustomerId))
            {
                logger.LogInformation("Seeding Test Customer profile...");
                context.Customers.Add(new Customer(testCustomerId, "Botse", "Modise"));
                await context.SaveChangesAsync();
            }

            if (!await context.Transactions.AnyAsync(t => t.CustomerId == testCustomerId))
            {
                logger.LogInformation("Seeding historical transactions for Velocity testing...");
                var baseTime = DateTime.UtcNow.AddMinutes(-10);

                for (int i = 1; i <= 5; i++)
                {
                    var txnId = $"TXN_{DateTime.UtcNow:yyyyMMdd}_00{i}";

                    context.Transactions.Add(new Transaction(
                        txnId,
                        testCustomerId,
                        450.00m,
                        currency: "ZAR",
                        baseTime.AddMinutes(i)
                    ));
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
