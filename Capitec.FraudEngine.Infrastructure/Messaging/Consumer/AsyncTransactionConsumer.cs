using Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransactions;
using Capitec.FraudEngine.Application.Features.Transactions.ProcessSyncTransaction;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Infrastructure.Messaging.Consumer
{
    public class AsyncTransactionConsumer(ISender mediator, ILogger<AsyncTransactionConsumer> logger): IConsumer<AsyncTransactionItem>
    {
        public async Task Consume(ConsumeContext<AsyncTransactionItem> context)
        {
            var queuedItem = context.Message;
            logger.LogInformation("Processing async transaction via MassTransit: {TxId}", queuedItem.TransactionId);

            var command = new ProcessSyncTransactionCommand(queuedItem.TransactionId,queuedItem.CustomerId,queuedItem.Amount,queuedItem.Currency);

            var result = await mediator.Send(command, context.CancellationToken);

            if (result.IsError)
            { 
                throw new InvalidOperationException($"Validation failed: {result.FirstError.Description}");
            }
        }
    }
}
