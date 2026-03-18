using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Persistence;
using Capitec.FraudEngine.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Tests.Infrastructure.Repository
{
    public class CustomerRepositoryTests
    {
        private FraudDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FraudDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            return new FraudDbContext(options);
        }

        [Fact]
        public async Task AddBatchAsync_WithMixedCustomers_OnlyInsertsNewCustomers()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new CustomerRepository(dbContext);

            // seed the database
            var existingCustomer = new Customer("CUST-001", "Existing", "User");
            dbContext.Customers.Add(existingCustomer);
            await dbContext.SaveChangesAsync();

            var incomingBatch = new List<Customer>
        {
            new("CUST-001", "Duplicate", "User"), 
            new("CUST-002", "Brand", "New")       
        };

            // Act
            await repository.AddBatchAsync(incomingBatch, CancellationToken.None);

            // Assert
            var totalCustomers = await dbContext.Customers.CountAsync();
            var savedCust002 = await dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == "CUST-002");

            Assert.Equal(2, totalCustomers); 
            Assert.NotNull(savedCust002);
        }
    }
}
