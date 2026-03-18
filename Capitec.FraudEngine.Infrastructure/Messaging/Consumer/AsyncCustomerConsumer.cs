using Capitec.FraudEngine.Application.Features.Customers.IngestAsyncCustomers;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Messaging.Consumer
{
    public class AsyncCustomerConsumer(ICustomerRepository repo, ILogger<AsyncCustomerConsumer> logger)
      : IConsumer<AsyncCustomerItem>
    {
        public async Task Consume(ConsumeContext<AsyncCustomerItem> context)
        {
            var msg = context.Message;

            logger.LogInformation("Background processing customer: {CustomerId}", msg.CustomerId);

            var customer = new Customer(msg.CustomerId, msg.FirstName, msg.LastName);

            await repo.AddBatchAsync(new List<Customer> { customer }, context.CancellationToken);
        }
    }
}
