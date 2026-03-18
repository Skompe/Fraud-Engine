using Capitec.FraudEngine.Application.Features.Investigations.IngestInvestigations;
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
    public class AsyncInvestigationConsumer(IFraudFlagRepository flagRepository,IUnitOfWork unitOfWork,ILogger<AsyncInvestigationConsumer> logger): IConsumer<InvestigationItem>
    {
        public async Task Consume(ConsumeContext<InvestigationItem> context)
        {
            var item = context.Message;
            logger.LogInformation("Processing async manual investigation for TxId: {TxId}", item.TransactionId);
   
            var fraudFlag = new FraudFlag(
                item.TransactionId,
                item.CustomerId,
                item.Status,
                item.Reason,
                item.Severity,
                item.Source,
                Array.Empty<string>() 
            );

            flagRepository.Add(fraudFlag);

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
    }
}
